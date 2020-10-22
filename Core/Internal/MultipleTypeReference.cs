﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A type that is the same as multiple different references.
    /// </summary>
    public sealed class MultipleTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Tries to create a new MultipleTypeReference.
        /// </summary>
        public static Result<ITypeReference, IErrorBuilder> TryCreate(IEnumerable<ITypeReference> references, string parentStep)
        {
            var set = references.ToImmutableHashSet();

            switch (set.Count)
            {
                case 0:
                    return new ActualTypeReference(typeof(object)); //TODO type reference any???
                case 1:
                    return Result.Success<ITypeReference, IErrorBuilder>(set.Single());
                default:
                {
                    if (set.OfType<ActualTypeReference>().Count() > 1)
                        return new ErrorBuilder(
                            $"Could not infer type for {parentStep} as it's children have different types ({string.Join(", ", set.OfType<ActualTypeReference>().Select(x=>x.Type.Name))}).",
                            ErrorCode.InvalidCast, null);

                    return new MultipleTypeReference(set);
                }
            }
        }

        /// <summary>
        /// Creates a new MultipleTypeReference.
        /// </summary>
        /// <param name="allReferences"></param>
        private MultipleTypeReference(ImmutableHashSet<ITypeReference> allReferences) => AllReferences = allReferences.ToImmutableHashSet();

        /// <summary>
        /// The type references.
        /// </summary>
        public ImmutableHashSet<ITypeReference> AllReferences { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference actualType => AllReferences.Count == 1 && AllReferences.Contains(actualType),
                MultipleTypeReference multipleTypeReference => AllReferences.SetEquals(multipleTypeReference
                    .AllReferences),
                VariableTypeReference variableTypeReference => AllReferences.Count == 1 && AllReferences.Contains(variableTypeReference),
                GenericTypeReference genericTypeReference => AllReferences.Count == 1 && AllReferences.Contains(genericTypeReference),
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => AllReferences.FirstOrDefault().GetHashCode();

        /// <inheritdoc />
        public IEnumerable<VariableTypeReference> VariableTypeReferences => AllReferences.SelectMany(x=>x.VariableTypeReferences);

        /// <inheritdoc />
        public Result<ActualTypeReference, IErrorBuilder> TryGetActualTypeReference(TypeResolver typeResolver)
        {
            var results = AllReferences
                .Select(x => x.TryGetActualTypeReference(typeResolver))
                .Combine(ErrorBuilderList.Combine)
                .Bind(x => x.Distinct().EnsureSingle("Type multiply defined")
                    .MapError(y => new ErrorBuilder(y, ErrorCode.AmbiguousType, null) as IErrorBuilder))
                ; //TODO improve this error

            return results;
        }

        /// <inheritdoc />
        public Result<ActualTypeReference, IErrorBuilder> TryGetGenericTypeReference(TypeResolver typeResolver, int argumentNumber)
        {
            var result = AllReferences
                .Select(x => x.TryGetGenericTypeReference(typeResolver, argumentNumber))
                .Combine(ErrorBuilderList.Combine)
                .Map(x => x.ToHashSet())
                .Ensure(x=>x.Count == 1,new ErrorBuilder("Type multiply defined", ErrorCode.AmbiguousType, null))
                .Map(x=>x.Single());

            return result;
        }
    }
}