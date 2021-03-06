﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using StepParameterDict =
    System.Collections.Generic.IReadOnlyDictionary<
        Reductech.EDR.Core.Internal.StepParameterReference,
        Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The data used by a Freezable Step.
/// </summary>
public sealed class FreezableStepData
{
    /// <summary>
    /// Creates a new FreezableStepData
    /// </summary>
    public FreezableStepData(StepParameterDict stepProperties, TextLocation location)
    {
        StepProperties = stepProperties;
        Location       = location;
    }

    /// <summary>
    /// The step properties.
    /// </summary>
    public StepParameterDict StepProperties { get; }

    /// <summary>
    /// The location where this data comes from.
    /// </summary>
    public TextLocation Location { get; }

    private Result<T, IError> TryGetValue<T>(
        string propertyName,
        Type stepType,
        Func<FreezableStepProperty, Result<T, IError>> extractValue)
    {
        var property = stepType.GetProperty(propertyName);

        if (property == null)
            throw new Exception($"{stepType.Name} does not have property {propertyName}");

        foreach (var reference in StepParameterReference.GetPossibleReferences(property))
            if (StepProperties.TryGetValue(reference, out var value))
                return extractValue(value);

        return Result.Failure<T, IError>(
            ErrorCode.MissingParameter.ToErrorBuilder(propertyName)
                .WithLocation(new ErrorLocation(stepType.Name, Location))
        );
    }

    /// <summary>
    /// Gets a variable name.
    /// </summary>
    public Result<VariableName, IError> TryGetVariableName(string propertyName, Type stepType) =>
        TryGetValue(
            propertyName,
            stepType,
            x =>
                x.AsVariableName(propertyName)
        );

    /// <summary>
    /// Gets a step argument
    /// </summary>
    public Result<IFreezableStep, IError> TryGetStep(string propertyName, Type stepType) =>
        TryGetValue(
            propertyName,
            stepType,
            x =>
                Result.Success<IFreezableStep, IError>(x.ConvertToStep())
        );

    /// <summary>
    /// Gets a step argument
    /// </summary>
    public Result<FreezableStepProperty.Lambda, IError> TryGetLambda(
        string propertyName,
        Type stepType) => TryGetValue(
        propertyName,
        stepType,
        x =>
            Result.Success<FreezableStepProperty.Lambda, IError>(x.ConvertToLambda())
    );

    /// <summary>
    /// Gets a variable name.
    /// </summary>
    public Result<IReadOnlyList<IFreezableStep>, IError> TryGetStepList(
        string propertyName,
        Type stepType) => TryGetValue(propertyName, stepType, x => x.AsStepList(propertyName));

    /// <inheritdoc />
    public override string ToString()
    {
        var keyString = string.Join("; ", StepProperties);

        if (string.IsNullOrWhiteSpace(keyString))
            return "Empty";

        return keyString;
    }

    /// <summary>
    /// Gets the variables set by steps in this FreezableStepData.
    /// </summary>
    public Result<IReadOnlyCollection<UsedVariable>, IError>
        GetVariablesUsed(string stepName, CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var variables   = new List<UsedVariable>();
        var errors      = new List<IError>();
        var stepFactory = typeResolver.StepFactoryStore.Dictionary[stepName];

        foreach (var (key, freezableStepProperty) in StepProperties)
        {
            switch (freezableStepProperty)
            {
                case FreezableStepProperty.Step step:
                    GetVariablesUsedByStep(step.FreezableStep, key, false);
                    break;
                case FreezableStepProperty.StepList stepList:
                {
                    foreach (var step in stepList.List)
                        GetVariablesUsedByStep(step, key, true);

                    break;
                }
                case FreezableStepProperty.Lambda lambda:
                {
                    GetVariablesUsedByLambda(lambda.FreezableStep, lambda.VName, key);
                    break;
                }
                case FreezableStepProperty.Variable vName:
                {
                    GetVariablesUsedByVariableName(vName, key);
                    break;
                }

                default: throw new ArgumentOutOfRangeException();
            }
        }

        if (errors.Any())
            return Result
                .Failure<IReadOnlyCollection<UsedVariable>,
                    IError>(ErrorList.Combine(errors));

        return variables;

        void GetVariablesUsedByVariableName(
            FreezableStepProperty.Variable vName,
            StepParameterReference stepParameterReference)
        {
            //Is this variable name actually a GetVariable step
            if (!stepFactory.ParameterDictionary.TryGetValue(
                stepParameterReference,
                out var parameter
            ))
            {
                errors.Add(
                    ErrorCode.UnexpectedParameter
                        .ToErrorBuilder(stepParameterReference.Name, stepName)
                        .WithLocationSingle(Location)
                );

                return;
            }

            if (parameter.GetCustomAttribute<StepPropertyAttribute>() is not null)
            {
                var step = vName.ConvertToStep();
                GetVariablesUsedByStep(step, stepParameterReference, false);
            }
        }

        void GetVariablesUsedByStep(
            IFreezableStep freezableStep,
            StepParameterReference stepParameterReference,
            bool isList)
        {
            if (!stepFactory.ParameterDictionary.TryGetValue(
                stepParameterReference,
                out var parameter
            ))
            {
                errors.Add(
                    ErrorCode.UnexpectedParameter
                        .ToErrorBuilder(stepParameterReference.Name, stepName)
                        .WithLocationSingle(Location)
                );

                return;
            }

            TypeReference tr;

            if (isList)
            {
                if (parameter.PropertyType.IsGenericType
                 && parameter.PropertyType.GetGenericTypeDefinition()
                 == typeof(IReadOnlyList<>))
                {
                    var stepType = parameter.PropertyType.GenericTypeArguments[0];
                    tr = TypeReference.CreateFromStepType(stepType);
                }

                else
                {
                    var arrayTypeReference =
                        TypeReference.CreateFromStepType(parameter.PropertyType);

                    var memberTypeReference =
                        arrayTypeReference.TryGetArrayMemberTypeReference(typeResolver);

                    if (memberTypeReference.IsFailure)
                    {
                        var badStep = StepProperties[stepParameterReference];

                        errors.Add(
                            ErrorCode.WrongType.ToErrorBuilder(
                                    stepName,
                                    arrayTypeReference.Name,
                                    parameter.Name,
                                    badStep.Location.Text,
                                    "Array/Sequence"
                                )
                                .WithLocation(badStep.Location)
                        );

                        return;
                    }

                    tr = memberTypeReference.Value;
                }
            }
            else if (parameter.GetCustomAttribute<FunctionPropertyAttribute>() is not null)
            {
                GetVariablesUsedByLambda(freezableStep, null, stepParameterReference);
                return;
            }
            else
            {
                tr = TypeReference.CreateFromStepType(parameter.PropertyType);
            }

            var childCallerMetadata = new CallerMetadata(stepName, stepParameterReference.Name, tr);
            //change caller metadata
            var variablesSet = freezableStep.GetVariablesUsed(childCallerMetadata, typeResolver);

            if (variablesSet.IsFailure)
                errors.Add(variablesSet.Error);
            else
                variables.AddRange(variablesSet.Value);
        }

        void GetVariablesUsedByLambda(
            IFreezableStep freezableStep,
            VariableName? lambdaVariable,
            StepParameterReference stepParameterReference)
        {
            if (!stepFactory.ParameterDictionary.TryGetValue(
                stepParameterReference,
                out var parameter
            ))
            {
                errors.Add(
                    ErrorCode.UnexpectedParameter
                        .ToErrorBuilder(stepParameterReference.Name, stepName)
                        .WithLocationSingle(Location)
                );

                return;
            }

            var tr = TypeReference.CreateFromStepType(parameter.PropertyType);

            var childCallerMetadata = new CallerMetadata(stepName, stepParameterReference.Name, tr);

            var vn           = lambdaVariable ?? VariableName.Item;
            var variablesSet = freezableStep.GetVariablesUsed(childCallerMetadata, typeResolver);

            if (variablesSet.IsFailure)
                errors.Add(variablesSet.Error);
            else
                variables.AddRange(variablesSet.Value.Where(x => x.VariableName != vn));
        }
    }
}

}
