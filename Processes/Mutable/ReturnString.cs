﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Returns a given string. Useful for testing.
    /// </summary>
    public class ReturnString : Process
    {
        /// <summary>
        /// The string to return.
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
#pragma warning disable 8618
        public string ResultString { get; set; }
#pragma warning restore 8618

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(String);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetReturnStringProcessName(ResultString);

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r=  new Immutable.ReturnString(ResultString);

            return TryConvertFreezeResult<TOutput, string>(r);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, string, TFinal, Immutable.ReturnString, ReturnString>(this);
        }
    }

}