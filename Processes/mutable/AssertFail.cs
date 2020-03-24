﻿using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    

    /// <summary>
    /// Asserts that a particular process will fail.
    /// </summary>
    public class AssertFail : Process
    {
        /// <summary>
        /// The process that is expected to fail.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Process { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override string GetName()
        {
            return $"Assert Fail: {Process?.GetName()}";
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            if (Process == null)
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"{nameof(Process)} is null."));

            var subProcessFreezeResult = Process.TryFreeze(processSettings);

            if (subProcessFreezeResult.IsFailure) return subProcessFreezeResult;

            var r = new immutable.AssertFail(GetName(), subProcessFreezeResult.Value);

            return Result.Success<ImmutableProcess, ErrorList>(r);
        }
    }
}