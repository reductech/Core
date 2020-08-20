﻿using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class Conditional : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            var result = Condition.Run(processState)
                .Bind(r =>
                {
                    if (r)
                        return ThenProcess.Run(processState);
                    return ElseProcess?.Run(processState) ?? Result.Success(Unit.Default);
                });

            return result;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ConditionalProcessFactory.Instance;

        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> ThenProcess { get; set; } = null!;

        //TODO else if
        //public IReadOnlyList<IRunnableProcess<Unit>> ElseIfProcesses

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<Unit>? ElseProcess { get; set; } = null;

    }

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class ConditionalProcessFactory : SimpleRunnableProcessFactory<Conditional, Unit>
    {
        private ConditionalProcessFactory() { }

        public static ConditionalProcessFactory Instance { get; } = new ConditionalProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"If [{nameof(Conditional.Condition)}] then [{nameof(Conditional.ThenProcess)}] else [{nameof(Conditional.ElseProcess)}]");
    }
}
