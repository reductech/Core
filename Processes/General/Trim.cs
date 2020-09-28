﻿using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class Trim : CompoundStep<string>
    {

        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The side to trim.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<TrimSide> Side { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad) =>
            String.Run(stateMonad).Compose(() => Side.Run(stateMonad))
                .Map(x => TrimString(x.Item1, x.Item2));

        private static string TrimString(string s, TrimSide side) =>
            side switch
            {
                TrimSide.Left => s.TrimStart(),
                TrimSide.Right => s.TrimEnd(),
                TrimSide.Both => s.Trim(),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

        /// <inheritdoc />
        public override IStepFactory StepFactory => TrimStepFactory.Instance;
    }
}