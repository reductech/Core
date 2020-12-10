﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using DateTime = System.DateTime;
using Option = OneOf.OneOf<Reductech.EDR.Core.Parser.StringStream, int, double, bool,
Reductech.EDR.Core.Internal.Enumeration, System.DateTime,
Reductech.EDR.Core.Entity,
Reductech.EDR.Core.Entities.EntityStream>;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public sealed class ConstantFreezableStep : IFreezableStep
    {
        /// <summary>
        /// Creates a new ConstantFreezableStep.
        /// </summary>
        /// <param name="value"></param>
        public ConstantFreezableStep(Option value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public Option Value { get; }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {
            var r = Value.Match(
                x => new StringConstant(x),
                x => new IntConstant(x),
                x => new DoubleConstant(x),
                x => new BoolConstant(x),
                x => GetEnumStep(),
                x => new DateTimeConstant(x),
                x => new EntityConstant(x),
                x => new EntityStreamConstant(x));

            return r;


            Result<IStep, IError> GetEnumStep()
            {
                var valueResult = GetValue(stepContext.TypeResolver.StepFactoryStore);

                if (valueResult.IsFailure) return valueResult.ConvertFailure<IStep>();

                return EnumConstantHelper.TryCreateEnumConstant(valueResult.Value)
                    .MapError(x => x.WithLocation(this));
            }


        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            return Result.Success<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError>(new List<(VariableName variableName, Maybe<ITypeReference>)>());
        }

        /// <inheritdoc />
        public string StepName
        {
            get
            {
                return Value.Match(
                            s=>s.Name,
                            i => i.ToString(),
                            d => d.ToString("G17"),
                            b => b.ToString(),
                            e => e.ToString(),
                            dt => dt.ToString("O"),
                            entity => entity.Serialize(),
                            es => "EntityStream"
                        );
            }
        }

        private Result<object, IError> GetValue(StepFactoryStore stepFactoryStore)
        {
            return Value.Match(
                Result.Success<object, IError>,
                x=>Result.Success<object, IError>(x),
                x=>Result.Success<object, IError>(x),
                x=>Result.Success<object, IError>(x),
                TryGetEnumerationValue,
                x=>Result.Success<object, IError>(x),
                Result.Success<object, IError>,
                Result.Success<object, IError>);

            Result<object, IError> TryGetEnumerationValue(Enumeration enumeration)
            {
                var type = TryGetType(stepFactoryStore);
                if (type.IsFailure) return type.ConvertFailure<object>();

                if (Enum.TryParse(type.Value, enumeration.Value, false, out var o))
                    return o!;

                return new SingleError($"Enum '{enumeration}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
            }
        }

        /// <summary>
        /// Serialize this constant.
        /// </summary>
        public async Task<string> Serialize(CancellationToken cancellation)
        {

            if (Value.IsT7)
                return await SerializationMethods.SerializeEntityStreamAsync(Value.AsT7, cancellation);


            if (Value.IsT0)
                return await Value.AsT0.SerializeAsync(cancellation);

            return
                Value.Match(
                    x=> throw new Exception("Should not encounter string stream here"),
                    i => i.ToString(),
                    d => d.ToString("G17"),
                    b => b.ToString(),
                    e => e.ToString(),
                    dt => dt.ToString("O"),
                    entity => entity.Serialize(),
                    es => throw new Exception("Should not encounter entity stream here"));
        }


        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => TryGetType(typeResolver.StepFactoryStore)
            .Map(x => new ActualTypeReference(x) as ITypeReference);

        private Result<Type, IError> TryGetType(StepFactoryStore stepFactoryStore)
        {
            var type = Value.Match(
               _ => typeof(StringStream),
               _ => typeof(int),
               _ => typeof(double),
               _ => typeof(bool),
               GetEnumerationType,
               _ => typeof(DateTime),
               _ => typeof(Entity),
               _ => typeof(EntityStream));

            return type;

            Result<Type, IError> GetEnumerationType(Enumeration enumeration)
            {
                if (stepFactoryStore.EnumTypesDictionary.TryGetValue(enumeration.Type, out var t))
                    return t;
                return new SingleError($"Enum '{enumeration.Type}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
            }
        }

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            var r= other is ConstantFreezableStep cfs && Value.Equals(cfs.Value);

            return r;
        }


        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();
    }
}