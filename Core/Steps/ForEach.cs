﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
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
/// Do an action for each member of the list.
/// </summary>
[Alias("EntityForEach")]
public sealed class ForEach<T> : CompoundStep<Unit>
{
    /// <summary>
    /// The elements to iterate over.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    public LambdaFunction<T, Unit> Action { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var elements = await Array.Run(stateMonad, cancellationToken);

        if (elements.IsFailure)
            return elements.ConvertFailure<Unit>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<Result<Unit, IError>> Apply(T element, CancellationToken cancellation)
        {
            var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Action.VariableNameOrItem,
                new KeyValuePair<VariableName, object>(Action.VariableNameOrItem, element!)
            );

            var result = await Action.StepTyped.Run(scopedMonad, cancellation);
            return result;
        }

        var finalResult = await elements.Value.ForEach(Apply, cancellationToken);

        return finalResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ForEachStepFactory.Instance;

    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    private sealed class ForEachStepFactory : ArrayStepFactory
    {
        private ForEachStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new ForEachStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ForEach<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata
                .CheckAllows(TypeReference.Unit.Instance, null)
                .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ForEach<object>.Array);

        /// <inheritdoc />
        protected override string LambdaPropertyName => nameof(ForEach<object>.Action);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);
    }
}

}
