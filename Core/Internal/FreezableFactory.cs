﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using StepParameterDict =
    System.Collections.Generic.Dictionary<Reductech.EDR.Core.Internal.StepParameterReference,
        Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Methods to create freezable types
/// </summary>
public static class FreezableFactory
{
    //TODO move other CreateFreezable methods here

    /// <summary>
    /// Create a new Freezable EntityGetValue
    /// </summary>
    public static IFreezableStep CreateFreezableArrayAccess(
        IFreezableStep entityOrArray,
        IFreezableStep indexer,
        Configuration? configuration,
        IErrorLocation location)
    {
        var entityGetValueDict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(EntityGetValue.Entity)),
                new FreezableStepProperty(entityOrArray, location)
            },
            {
                new StepParameterReference(nameof(EntityGetValue.Property)),
                new FreezableStepProperty(indexer, location)
            },
        };

        var entityGetValueData = new FreezableStepData(entityGetValueDict, location);

        var entityGetValueStep = new CompoundFreezableStep(
            EntityGetValueStepFactory.Instance.TypeName,
            entityGetValueData,
            configuration
        );

        var elementAtIndexDict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(ElementAtIndex<object>.Array)),
                new FreezableStepProperty(entityOrArray, location)
            },
            {
                new StepParameterReference(nameof(ElementAtIndex<object>.Index)),
                new FreezableStepProperty(indexer, location)
            },
        };

        var elementAtData = new FreezableStepData(elementAtIndexDict, location);

        var elementAtStep = new CompoundFreezableStep(
            ElementAtIndexStepFactory.Instance.TypeName,
            elementAtData,
            configuration
        );

        var result = new OptionFreezableStep(new[] { entityGetValueStep, elementAtStep });
        return result;
    }

    /// <summary>
    /// Create a new Freezable Sequence
    /// </summary>
    public static IFreezableStep CreateFreezableSequence(
        IEnumerable<IFreezableStep> steps,
        IFreezableStep finalStep,
        Configuration? configuration,
        IErrorLocation location)
    {
        var dict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(Sequence<object>.InitialSteps)),
                new FreezableStepProperty(steps.ToImmutableList(), location)
            },
            {
                new StepParameterReference(nameof(Sequence<object>.FinalStep)),
                new FreezableStepProperty(finalStep, location)
            },
        };

        var fpd = new FreezableStepData(dict, location);

        return new CompoundFreezableStep(SequenceStepFactory.Instance.TypeName, fpd, configuration);
    }

    /// <summary>
    /// Create a freezable GetVariable step.
    /// </summary>
    public static IFreezableStep CreateFreezableGetVariable(
        VariableName variableName,
        IErrorLocation location)
    {
        var dict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(GetVariable<object>.Variable)),
                new FreezableStepProperty(variableName, location)
            }
        };

        var fpd  = new FreezableStepData(dict, location);
        var step = new CompoundFreezableStep(GetVariableStepFactory.Instance.TypeName, fpd, null);

        return step;
    }

    /// <summary>
    /// Create a freezable GetVariable step.
    /// </summary>
    public static IFreezableStep CreateFreezableSetVariable(
        FreezableStepProperty variableName,
        FreezableStepProperty value,
        IErrorLocation location)
    {
        var dict = new StepParameterDict
        {
            { new StepParameterReference(nameof(SetVariable<object>.Variable)), variableName },
            { new StepParameterReference(nameof(SetVariable<object>.Value)), value },
        };

        var fpd  = new FreezableStepData(dict, location);
        var step = new CompoundFreezableStep(SetVariableStepFactory.Instance.TypeName, fpd, null);

        return step;
    }

    /// <summary>
    /// Create a freezable Not step.
    /// </summary>
    public static IFreezableStep CreateFreezableNot(IFreezableStep boolean, IErrorLocation location)
    {
        var dict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(Not.Boolean)),
                new FreezableStepProperty(boolean, location)
            },
        };

        var fpd  = new FreezableStepData(dict, location);
        var step = new CompoundFreezableStep(NotStepFactory.Instance.TypeName, fpd, null);

        return step;
    }

    /// <summary>
    /// Create a new Freezable Array
    /// </summary>
    public static IFreezableStep CreateFreezableList(
        ImmutableList<IFreezableStep> elements,
        Configuration? configuration,
        IErrorLocation location)
    {
        var dict = new StepParameterDict
        {
            {
                new StepParameterReference(nameof(ArrayNew<object>.Elements)),
                new FreezableStepProperty(elements, location)
            }
        };

        var fpd = new FreezableStepData(dict, location);

        return new CompoundFreezableStep(ArrayNewStepFactory.Instance.TypeName, fpd, configuration);
    }
}

/// <summary>
/// Contains helper methods for creating infix steps
/// </summary>
public static class InfixHelper
{
    private record OperatorData(
        string StepName,
        string LeftName,
        string RightName,
        (string Name, IFreezableStep Step)? Operator) { }

    /// <summary>
    /// Creates an infix operator step
    /// </summary>
    public static Result<FreezableStepProperty, IError> TryCreateStep(
        IErrorLocation errorLocation,
        Result<FreezableStepProperty, IError> left,
        Result<FreezableStepProperty, IError> right,
        string op)
    {
        List<IError> errors = new();

        if (left.IsFailure)
            errors.Add(left.Error);

        if (right.IsFailure)
            errors.Add(right.Error);

        if (!OperatorDataDictionary.TryGetValue(op, out var opData))
            errors.Add(new SingleError(errorLocation, ErrorCode.CouldNotParse, op, "Operator"));

        if (errors.Any())
            return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

        var stepParameterDict = new StepParameterDict
        {
            { new StepParameterReference(opData!.LeftName), left.Value },
            { new StepParameterReference(opData.RightName), right.Value },
        };

        if (opData.Operator.HasValue)
        {
            stepParameterDict.Add(
                new StepParameterReference(opData.Operator.Value.Name),
                new FreezableStepProperty(opData.Operator.Value.Step, errorLocation)
            );
        }

        var data = new FreezableStepData(
            stepParameterDict,
            errorLocation
        );

        var step = new CompoundFreezableStep(opData.StepName, data, null);

        return new FreezableStepProperty(step, errorLocation);
    }

    private static readonly IReadOnlyDictionary<string, OperatorData> OperatorDataDictionary =
        Enum.GetValues<MathOperator>()
            .ToDictionary(
                mo => mo.GetDisplayName(),
                mo => new OperatorData(
                    nameof(ApplyMathOperator),
                    nameof(ApplyMathOperator.Left),
                    nameof(ApplyMathOperator.Right),
                    (
                        nameof(ApplyMathOperator.Operator),
                        new EnumConstantFreezable(
                            new Enumeration(nameof(MathOperator), mo.ToString())
                        ))
                )
            )
            .Concat(
                Enum.GetValues<BooleanOperator>()
                    .ToDictionary(
                        mo => mo.GetDisplayName(),
                        mo => new OperatorData(
                            nameof(ApplyBooleanOperator),
                            nameof(ApplyBooleanOperator.Left),
                            nameof(ApplyBooleanOperator.Right),
                            (
                                nameof(ApplyBooleanOperator.Operator),
                                new EnumConstantFreezable(
                                    new Enumeration(nameof(BooleanOperator), mo.ToString())
                                ))
                        )
                    )
            )
            .Concat(
                Enum.GetValues<CompareOperator>()
                    .ToDictionary(
                        mo => mo.GetDisplayName(),
                        mo => new OperatorData(
                            CompareStepFactory.Instance.TypeName,
                            nameof(Compare<int>.Left),
                            nameof(Compare<int>.Right),
                            (
                                nameof(Compare<int>.Operator),
                                new EnumConstantFreezable(
                                    new Enumeration(nameof(CompareOperator), mo.ToString())
                                ))
                        )
                    )
            )
            .Append(
                new KeyValuePair<string, OperatorData>(
                    "with",
                    new OperatorData(
                        EntityCombineStepFactory.Instance.TypeName,
                        nameof(EntityCombine.First),
                        nameof(EntityCombine.Second),
                        null
                    )
                )
            )
            .Where(x => x.Key != MathOperator.None.ToString())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
}

}
