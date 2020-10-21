﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class Unzip : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var data = await ArchiveFilePath.Run(stateMonad, cancellationToken)
                .Compose(() => DestinationDirectory.Run(stateMonad, cancellationToken), () => OverwriteFiles.Run(stateMonad, cancellationToken));

            if (data.IsFailure)
                return data.ConvertFailure<Unit>();

            Maybe<IError> error;
            try
            {
                ZipFile.ExtractToDirectory(data.Value.Item1, data.Value.Item2, data.Value.Item3);
                error = Maybe<IError>.None;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                error = Maybe<IError>.From(new SingleError(e.Message, Name, null, ErrorCode.ExternalProcessError));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (error.HasValue)
                return Result.Failure<Unit, IError>(error.Value);

            return Unit.Default;

        }


        /// <summary>
        /// The path to the archive to unzip.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> ArchiveFilePath { get; set; } = null!;

        /// <summary>
        /// The directory to unzip to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> DestinationDirectory { get; set; } = null!;

        /// <summary>
        /// Whether to overwrite files when unzipping.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("false")]
        public IStep<bool> OverwriteFiles { get; set; } = new Constant<bool>(false);

        /// <inheritdoc />
        public override IStepFactory StepFactory => UnzipStepFactory.Instance;
    }

    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class UnzipStepFactory : SimpleStepFactory<Unzip, Unit>
    {
        private UnzipStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Unzip, Unit> Instance { get; } = new UnzipStepFactory();
    }
}
