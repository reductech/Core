﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the letters that appears at a specific index
/// </summary>
[SCLExample("CharAtIndex 'hello' 1", "e")]
public sealed class CharAtIndex : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to extract a substring from.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The index.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<int> Index { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var index = await Index.Run(stateMonad, cancellationToken);

        if (index.IsFailure)
            return index.ConvertFailure<StringStream>();

        var stringStreamResult = await String.Run(stateMonad, cancellationToken);

        if (stringStreamResult.IsFailure)
            return stringStreamResult;

        var str = await stringStreamResult.Value.GetStringAsync();

        if (index.Value < 0 || index.Value >= str.Length)
            return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);

        var character = str[index.Value].ToString();

        return new StringStream(character);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<CharAtIndex, StringStream>();
}

}
