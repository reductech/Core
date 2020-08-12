﻿using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public class NotProcessFactory : SimpleRunnableProcessFactory<NotProcessFactory.Not, bool>
    {
        private NotProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new NotProcessFactory();


        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Not [{nameof(Not.Boolean)}]";


        /// <summary>
        /// Negation of a boolean value.
        /// </summary>
        public sealed class Not : CompoundRunnableProcess<bool>
        {
            /// <inheritdoc />
            public override Result<bool> Run(ProcessState processState) => Boolean.Run(processState).Map(x => !x);

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => NotProcessFactory.Instance;

            /// <summary>
            /// The value to negate.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Boolean { get; set; }
        }
    }
}