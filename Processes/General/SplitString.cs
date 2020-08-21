﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Splits a string.
    /// </summary>
    public sealed class SplitString : CompoundRunnableProcess<List<string>>
    {
        /// <summary>
        /// The string to split.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Delimiter { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<string>, IRunErrors> Run(ProcessState processState) =>
            String.Run(processState).Compose(() => Delimiter.Run(processState))
                .Map(x => x.Item1.Split(new[] {x.Item2}, StringSplitOptions.None).ToList());

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => SplitStringProcessFactory.Instance;
    }

    /// <summary>
    /// Splits a string.
    /// </summary>
    public class SplitStringProcessFactory : SimpleRunnableProcessFactory<SplitString, List<string>>
    {
        private SplitStringProcessFactory() { }

        public static SimpleRunnableProcessFactory<SplitString, List<string>> Instance { get; } = new SplitStringProcessFactory();
    }
}