﻿using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step which can be Runnable.
    /// </summary>
    public interface IFreezableStep
    {
        /// <summary>
        /// Try to freeze this step.
        /// </summary>
        Result<IStep> TryFreeze(StepContext stepContext);

        /// <summary>
        /// Gets the variables which may be set by this step and their types.
        /// </summary>
        Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet(TypeResolver typeResolver);


        /// <summary>
        /// The human-readable name of this step.
        /// </summary>
        string StepName { get; }


        /// <summary>
        /// The output type of this step. Will be unit if the step does not have an output.
        /// </summary>
        /// <param name="typeResolver"></param>
        Result<ITypeReference> TryGetOutputTypeReference(TypeResolver typeResolver);
    }
}