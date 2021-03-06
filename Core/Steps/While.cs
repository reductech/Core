﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Repeat an action while the condition is met.
/// </summary>
public sealed class While : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var conditionResult = await Condition.Run(stateMonad, cancellationToken);

            if (conditionResult.IsFailure)
                return conditionResult.ConvertFailure<Unit>();

            if (conditionResult.Value)
            {
                var actionResult = await Action.Run(stateMonad, cancellationToken);

                if (actionResult.IsFailure)
                    return actionResult.ConvertFailure<Unit>();
            }
            else
                break;
        }

        return Unit.Default;
    }

    /// <summary>
    /// The condition to check before performing the action.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<bool> Condition { get; set; } = null!;

    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Unit> Action { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<While, Unit>();
}

}
