﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// A step that declares a new array
/// </summary>
public interface IArrayNewStep
{
    /// <summary>
    /// The elements of the array
    /// </summary>
    IEnumerable<IStep> ElementSteps { get; } //This is used by ChainInfixSerializer
}

/// <summary>
/// Represents an ordered collection of objects.
/// </summary>
[Alias("Array")]
[Alias("NewArray")]
public sealed class ArrayNew<T> : CompoundStep<Array<T>>, IArrayNewStep
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Elements.Select(x => x.Run(stateMonad, cancellationToken))
            .Combine(ErrorList.Combine)
            .Map(x => x.ToList().ToSCLArray());

        return result;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// The elements of the array.
    /// </summary>
    [StepListProperty(1)]
    [Required]
    public IReadOnlyList<IStep<T>> Elements { get; set; } = null!;

    /// <inheritdoc />
    public IEnumerable<IStep> ElementSteps => Elements;

    /// <inheritdoc />
    public override Maybe<EntityValue> TryConvertToEntityValue()
    {
        var builder = ImmutableList<EntityValue>.Empty.ToBuilder();

        foreach (var element in Elements)
        {
            var ev = element.TryConvertToEntityValue();

            if (ev.HasNoValue)
                return Maybe<EntityValue>.None;

            builder.Add(ev.Value);
        }

        return new EntityValue.NestedList(builder.ToImmutable());
    }

    /// <summary>
    /// Creates an array.
    /// </summary>
    public static ArrayNew<T> CreateArray(List<IStep<T>> stepList)
    {
        return new() { Elements = stepList };
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayNewStepFactory.Instance;

    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    private class ArrayNewStepFactory : GenericStepFactory
    {
        private ArrayNewStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayNewStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayNew<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var mtr = callerMetadata.ExpectedType.TryGetArrayMemberTypeReference(typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (mtr.IsFailure)
                return mtr.ConvertFailure<TypeReference>();

            var result =
                freezableStepData.TryGetStepList(nameof(ArrayNew<object>.Elements), StepType)
                    .Bind(
                        x => x.Select(
                                r => r.TryGetOutputTypeReference(
                                    new CallerMetadata(
                                        TypeName,
                                        nameof(ArrayNew<int>.Elements),
                                        mtr.Value
                                    ),
                                    typeResolver
                                )
                            )
                            .Combine(ErrorList.Combine)
                    )
                    .Map(TypeReference.Create);

            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArraySerializer.Instance;
    }
}

}
