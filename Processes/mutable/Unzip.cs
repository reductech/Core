﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Unzips a file.
    /// </summary>
    public class Unzip : Process
    {
        /// <summary>
        /// The path to the archive to unzip.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ArchiveFilePath { get; set; }


        /// <summary>
        /// The path to the directory in which to place the extracted files.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public string DestinationDirectory { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Should files be overwritten in the destination directory.
        /// </summary>
        [YamlMember(Order = 3)]
        public bool OverwriteFiles { get; } = false;

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetUnzipName();

        /// <inheritdoc />
        public override Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze());
        }

        private Result<ImmutableProcess<Unit>> TryFreeze()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ArchiveFilePath))
                errors.Add($"{nameof(ArchiveFilePath)} is empty.");

            if (string.IsNullOrWhiteSpace(DestinationDirectory))
                errors.Add($"{nameof(DestinationDirectory)} is empty.");

            if (errors.Any())
                return Result.Failure<ImmutableProcess<Unit>>(string.Join("\r\n", errors));

            var ip = new immutable.Unzip(ArchiveFilePath, DestinationDirectory, OverwriteFiles);

            return Result.Success<ImmutableProcess<Unit>>(ip);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,immutable.Unzip,Unzip>(this);
        }
    }
}