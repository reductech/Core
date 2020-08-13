﻿using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class Test<T> : CompoundRunnableProcess<T>
    {
        /// <inheritdoc />
        public override Result<T> Run(ProcessState processState)
        {
            var result = Condition.Run(processState)
                .Bind(r => r ? ThenValue.Run(processState) : ElseValue.Run(processState));

            return result;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => TestSequenceFactory.Instance;


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
        public IRunnableProcess<T> ThenValue { get; set; } = null!;

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<T> ElseValue { get; set; } = null!;
    }

    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class TestSequenceFactory : GenericProcessFactory
    {
        private TestSequenceFactory() { }

        public static GenericProcessFactory Instance { get; } = new TestSequenceFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Test<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Test<object>.ThenValue))
                .Compose(() => freezableProcessData.GetArgument(nameof(Test<object>.ElseValue)))
                .Bind(x => x.Item1.TryGetOutputTypeReference().Compose(() => x.Item2.TryGetOutputTypeReference()))
                .Bind(x => MultipleTypeReference.TryCreate(new[] { x.Item1, x.Item2 }, TypeName));
    }
}