﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Calculates the length of the string.
/// </summary>
public sealed class StringLength : CompoundStep<int>
{
    /// <summary>
    /// The string to measure the length of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<int>();

        return str.Value.Length;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<StringLength, int>();
}

}
