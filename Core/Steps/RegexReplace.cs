﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Replace every regex match in the string with the result of a particular function
/// </summary>
public sealed class RegexReplace : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult =
            await String.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<StringStream>();

        var patternResult =
            await Pattern.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (patternResult.IsFailure)
            return patternResult.ConvertFailure<StringStream>();

        var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

        if (ignoreCaseResult.IsFailure)
            return ignoreCaseResult.ConvertFailure<StringStream>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        var regexOptions = RegexOptions.None;

        if (ignoreCaseResult.Value)
            regexOptions |= RegexOptions.IgnoreCase;

        var regex     = new Regex(patternResult.Value, regexOptions);
        var input     = stringResult.Value;
        var sb        = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in regex.Matches(input))
        {
            sb.Append(input, lastIndex, match.Index - lastIndex);

            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Function.VariableNameOrItem,
                new KeyValuePair<VariableName, object>(
                    Function.VariableNameOrItem,
                    new StringStream(match.Value)
                )
            );

            var result = await Function.StepTyped.Run(scopedMonad, cancellationToken)
                .Map(x => x.GetStringAsync());

            if (result.IsFailure)
                return result.ConvertFailure<StringStream>();

            sb.Append(result.Value);

            lastIndex = match.Index + match.Length;
        }

        sb.Append(input, lastIndex, input.Length - lastIndex);
        return new StringStream(sb.ToString());
    }

    /// <summary>
    /// The string to match
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The regular expression pattern.
    /// Uses the .net flavor
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> Pattern { get; set; } = null!;

    /// <summary>
    /// A function to take the regex match and return the new string
    /// </summary>
    [FunctionProperty(3)]
    [Required]
    public LambdaFunction<StringStream, StringStream> Function { get; set; } = null!;

    /// <summary>
    /// Whether the regex should ignore case.
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("False")]
    public IStep<bool> IgnoreCase { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RegexReplace, StringStream>();
}

}
