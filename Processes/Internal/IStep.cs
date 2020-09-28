﻿using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A step that can be run.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// The name of this step.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Convert this Step into a FreezableStep.
        /// </summary>
        IFreezableStep Unfreeze();

        /// <summary>
        /// Run this step and return the result untyped.
        /// </summary>
        Result<T, IRunErrors> Run<T>(StateMonad stateMonad);

        /// <summary>
        /// Verify that this step can be run with the current settings.
        /// </summary>
        public Result<Unit, IRunErrors> Verify(ISettings settings);

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        Configuration? Configuration { get; set; }

        /// <summary>
        /// Step combiners that could be used for this step.
        /// </summary>
        IEnumerable<IStepCombiner> StepCombiners { get; }

    }

    /// <summary>
    /// A step that can be run.
    /// </summary>
    public interface IStep<T> : IStep
    {
        /// <summary>
        /// Run this step and return the result.
        /// </summary>
        Result<T, IRunErrors> Run(StateMonad stateMonad); //TODO async
    }
}