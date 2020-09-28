﻿using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a variable name in a serialization.
    /// </summary>
    public class VariableNameComponent : ISerializerBlock, IProcessSerializerComponent
    {
        /// <summary>
        /// Deserializes a regex group into a Variable Name.
        /// </summary>
        public VariableNameComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) =>
            data.GetVariableName(PropertyName)
                .Bind(Serialize);

        /// <summary>
        /// Serialize a variable name.
        /// </summary>
        public static Result<string> Serialize(VariableName vn) => $"<{vn.Name}>";


        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}