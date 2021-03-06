﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Runs processes from Text
/// </summary>
public sealed class SCLRunner
{
    /// <summary>
    /// Creates a new SCL Runner
    /// </summary>
    public SCLRunner(
        ILogger logger,
        StepFactoryStore stepFactoryStore,
        IExternalContext externalContext)
    {
        _logger           = logger;
        _stepFactoryStore = stepFactoryStore;
        _externalContext  = externalContext;
    }

    private readonly ILogger _logger;
    private readonly StepFactoryStore _stepFactoryStore;
    private readonly IExternalContext _externalContext;

    /// <summary>
    /// Run step defined in an SCL string.
    /// </summary>
    /// <param name="text">SCL representing the step.</param>
    /// <param name="sequenceMetadata">Additional information about the sequence</param>
    /// <param name="cancellationToken">Cancellation ErrorLocation</param>
    /// <returns></returns>
    public async Task<Result<Unit, IError>> RunSequenceFromTextAsync(
        string text,
        Dictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken)
    {
        sequenceMetadata[SCLTextName]    = text;
        sequenceMetadata[SequenceIdName] = Guid.NewGuid();
        return await RunSequence(text, sequenceMetadata, cancellationToken);
    }

    /// <summary>
    /// The name of the SequenceId log property.
    /// </summary>
    public const string SequenceIdName = "SequenceId";

    /// <summary>
    /// The name of the SCL Path log property
    /// </summary>
    public const string SCLPathName = "SCLPath";

    /// <summary>
    /// The name of the SCL Text log property
    /// </summary>
    public const string SCLTextName = "SCLText";

    /// <summary>
    /// The top level logging scope.
    /// </summary>
    public const string TopLevelLoggingScope = "EDR";

    /// <summary>
    /// Caller metadata for the entire sequence.
    /// </summary>
    public static readonly CallerMetadata RootCallerMetadata = new(
        "Sequence",
        "Root",
        TypeReference.Any.Instance
    );

    /// <summary>
    /// Runs an SCL sequence without injecting any metadata
    /// </summary>
    public async Task<Result<Unit, IError>> RunSequence(
        string text,
        IReadOnlyDictionary<string, object> sequenceMetadata,
        CancellationToken cancellationToken)
    {
        var stepResult = SCLParsing.TryParseStep(text)
            .Bind(x => x.TryFreeze(RootCallerMetadata, _stepFactoryStore))
            .Map(ConvertToUnitStep);

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Unit>();

        using var loggingScope = _logger.BeginScope(TopLevelLoggingScope);

        var stateMonad = new StateMonad(
            _logger,
            _stepFactoryStore,
            _externalContext,
            sequenceMetadata
        );

        LogSituation.SequenceStarted.Log(stateMonad, null);

        if (_stepFactoryStore.ConnectorData.Any())
        {
            LogSituation.ConnectorSettings.Log(
                stateMonad,
                null,
                stateMonad.Settings.Serialize()
            );
        }

        var runResult = await stepResult.Value.Run(stateMonad, cancellationToken);

        await stateMonad.DisposeAsync();

        _logger.LogSituation(
            LogSituation.SequenceCompleted,
            null,
            sequenceMetadata,
            Array.Empty<object?>()
        );

        return runResult;
    }

    /// <summary>
    /// Converts the step to a unit step for running.
    /// </summary>
    public static IStep<Unit> ConvertToUnitStep(IStep step)
    {
        if (step is IStep<Unit> unitStep)
            return unitStep;

        IStep<Unit> log = SurroundWithLog(step as dynamic);
        return log;
    }

    private static IStep<Unit> SurroundWithLog<T>(IStep<T> step)
    {
        var p = new Log<T> { Value = step };

        return p;
    }
}

}
