﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// A process factory that uses default values for most properties.
    /// </summary>
    public abstract class SimpleRunnableProcessFactory<TProcess, TOutput> : RunnableProcessFactory where TProcess : IRunnableProcess, new ()
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
            => new ActualTypeReference(typeof(TOutput));

        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilder(ProcessNameTemplate);

        /// <summary>
        /// The template to use with the ProcessNameBuilder.
        /// </summary>
        protected abstract string ProcessNameTemplate { get; }

        /// <inheritdoc />
        public override string TypeName => FormatTypeName(typeof(TProcess));


        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            new TProcess();
    }
}