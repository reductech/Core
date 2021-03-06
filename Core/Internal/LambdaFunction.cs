﻿namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A lambda function with typed parameters
/// </summary>
public record LambdaFunction<TInput, TOutput> : LambdaFunction
{
    /// <summary>
    /// The Step to execute
    /// </summary>
    public IStep<TOutput> StepTyped { get; }

    /// <summary>
    /// A lambda function
    /// </summary>
    public LambdaFunction(VariableName? variable, IStep<TOutput> stepTyped) : base(
        variable,
        stepTyped
    )
    {
        StepTyped = stepTyped;
    }
}

/// <summary>
/// A lambda function
/// </summary>
public record LambdaFunction
{
    /// <summary>
    /// A lambda function
    /// </summary>
    protected LambdaFunction(VariableName? variable, IStep step)
    {
        this.Variable = variable;
        this.Step     = step;
    }

    /// <summary>
    /// Serialize this Lambda function
    /// </summary>
    public string Serialize()
    {
        var stepSerialized = Step.Serialize();

        if (Variable is null)
        {
            return $"<> => {stepSerialized}";
        }

        return $"{Variable.Value.Serialize()} => {stepSerialized}";
    }

    /// <summary>
    /// The VariableName or The Item variable name
    /// </summary>
    public VariableName VariableNameOrItem => Variable ?? VariableName.Item;

    /// <summary>
    /// The VariableName to use inside the function
    /// </summary>
    public VariableName? Variable { get; init; }

    /// <summary>
    /// The step
    /// </summary>
    public IStep Step { get; init; }
}

}
