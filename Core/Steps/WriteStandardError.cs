﻿using System;
using System.ComponentModel.DataAnnotations;
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
/// Writes to the console standard error
/// </summary>
[SCLExample(
    "WriteStandardError 'Something Went Wrong'",
    Description    = "Writes to the Standard Error",
    ExecuteInTests = false
)]
public class WriteStandardError : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var data = await Data.Run(stateMonad, cancellationToken);

        if (data.IsFailure)
            return data.ConvertFailure<Unit>();

        var (stream, _) = data.Value.GetStream();

        try
        {
            await stream.CopyToAsync(
                stateMonad.ExternalContext.Console.OpenStandardError(),
                cancellationToken
            );
        }
        catch (Exception e)
        {
            return Result.Failure<Unit, IError>(
                ErrorCode.ExternalProcessError.ToErrorBuilder(e).WithLocation(this)
            );
        }

        return Unit.Default;
    }

    /// <summary>
    /// The data to write
    /// </summary>

    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Data { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<WriteStandardError, Unit>();
}

}
