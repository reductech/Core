﻿using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Either a type itself, or the name of a variable with the same type.
    /// </summary>
    public interface ITypeReference
    {
        /// <summary>
        /// Gets the variable type references.
        /// </summary>
        IEnumerable<VariableTypeReference> VariableTypeReferences { get; }


        /// <summary>
        /// Tries to get actual type references
        /// </summary>
        Result<ActualTypeReference> TryGetActualTypeReference(TypeResolver typeResolver);

        /// <summary>
        /// Tries to get the generic member type
        /// </summary>
        Result<ActualTypeReference> TryGetGenericTypeReference(TypeResolver typeResolver, int argumentNumber);
    }


}