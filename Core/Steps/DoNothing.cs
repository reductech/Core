﻿using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Does nothing.
/// </summary>
[SCLExample("DoNothing", description: "Does nothing")]
public class DoNothing : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Unit.Default;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<DoNothing, Unit>();
}

}
