﻿using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the current version of the application
/// </summary>
public sealed class GetApplicationVersion : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is null)
            return new StringStream("Unknown Version");

        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(entryAssembly.Location);
        string          version         = fileVersionInfo.ProductVersion!;

        var name = entryAssembly.GetName().Name;

        var text = $"{name} {version}";

        return new StringStream(text);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetApplicationVersion, StringStream>();
}

}
