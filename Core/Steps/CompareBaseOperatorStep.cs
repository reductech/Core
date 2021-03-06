﻿using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Base class for compare operations
/// </summary>
public abstract class
    CompareBaseOperatorStep<TStep, TElement> : BaseOperatorStep<TStep, TElement,
        bool>
    where TStep : BaseOperatorStep<TStep, TElement, bool>, new()
{
    /// <summary>
    /// Check the result of comparing a term with the next term
    /// -1 means less than
    /// 0 means equals
    /// 1 means greater than
    /// </summary>
    protected abstract bool CheckComparisonValue(int v);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = CompareOperatorStepFactory.Instance;

    /// <inheritdoc />
    protected override Result<bool, IErrorBuilder> Operate(IEnumerable<TElement> terms)
    {
        var last     = Maybe<TElement>.None;
        var comparer = Comparer<TElement>.Default;

        foreach (var term in terms)
        {
            if (last.HasValue)
            {
                var comparisonValue = comparer.Compare(last.Value, term);
                var checkResult     = CheckComparisonValue(comparisonValue);

                if (!checkResult)
                    return false;
            }

            last = term;
        }

        return true;
    }

    /// <summary>
    /// Step factory for operators
    /// </summary>
    protected sealed class CompareOperatorStepFactory : GenericStepFactory
    {
        private CompareOperatorStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static CompareOperatorStepFactory Instance { get; } = new();

        /// <inheritdoc />
        public override Type StepType => typeof(TStep).GetGenericTypeDefinition();

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Boolean";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => TypeReference.Actual.Bool;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var checkResult = callerMetadata
                .CheckAllows(TypeReference.Actual.Bool, typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (checkResult.IsFailure)
                return checkResult.ConvertFailure<TypeReference>();

            var result = freezableStepData
                .TryGetStep(nameof(Terms), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(
                            TypeName,
                            nameof(Terms),
                            new TypeReference.Array(TypeReference.Any.Instance)
                        ),
                        typeResolver
                    )
                )
                .Bind(
                    x => x.TryGetArrayMemberTypeReference(typeResolver)
                        .MapError(e => e.WithLocation(freezableStepData))
                );

            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new ChainInfixSerializer(
            FormatTypeName(typeof(TStep)),
            new TStep().Operator
        );
    }
}

}
