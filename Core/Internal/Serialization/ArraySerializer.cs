﻿using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Serializes an array
/// </summary>
public sealed class ArraySerializer : IStepSerializer
{
    private ArraySerializer() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IStepSerializer Instance { get; } = new ArraySerializer();

    /// <inheritdoc />
    public string Serialize(IEnumerable<StepProperty> stepProperties) =>
        stepProperties.Single().Serialize();
}

}
