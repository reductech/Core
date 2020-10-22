﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class Compare<T> : CompoundStep<bool> where T : IComparable
    {
        /// <summary>
        /// The item to the left of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Left { get; set; } = null!;

        /// <summary>
        /// The operator to use for comparison.
        /// </summary>
        [StepProperty]
        [Required]

        public IStep<CompareOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The item to the right of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Right { get; set; } = null!;


        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await Left.Run(stateMonad, cancellationToken)
                .Compose(() => Operator.Run(stateMonad, cancellationToken), () => Right.Run(stateMonad, cancellationToken))
                .Bind(x => CompareItems(x.Item1, x.Item2, x.Item3));


            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => CompareStepFactory.Instance;

        private static Result<bool, IError> CompareItems(T item1, CompareOperator compareOperator, T item2)
        {
            return compareOperator switch
            {
                CompareOperator.Equals => item1.Equals(item2),
                CompareOperator.NotEquals => !item1.Equals(item2),
                CompareOperator.LessThan => item1.CompareTo(item2) < 0,
                CompareOperator.LessThanOrEqual => item1.CompareTo(item2) <= 0,
                CompareOperator.GreaterThan => item1.CompareTo(item2) > 0,
                CompareOperator.GreaterThanOrEqual => item1.CompareTo(item2) >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(compareOperator), compareOperator, null)
            };
        }

    }

    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class CompareStepFactory : GenericStepFactory
    {
        private CompareStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new CompareStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Compare<>);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(CompareOperator) };

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var result = freezableStepData.GetArgument(nameof(Compare<int>.Left))
                .MapError(e=>e.WithLocation(this, freezableStepData))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Compose(() => freezableStepData.GetArgument(nameof(Compare<int>.Right))
                .MapError(e=>e.WithLocation(this, freezableStepData))
                    .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                )
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName)
                .MapError(e=>e.WithLocation(this, freezableStepData)));

            return result;
        }

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(Compare<int>.Left)}] [{nameof(Compare<int>.Operator)}] [{nameof(Compare<int>.Right)}]");


        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new FixedStringComponent("("),
            new IntegerComponent(nameof(Compare<int>.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<CompareOperator>(nameof(Compare<int>.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(Compare<int>.Right)),
            new FixedStringComponent(")"));


        /// <summary>
        /// Create a freezable Compare step.
        /// </summary>
        public static IFreezableStep CreateFreezable(IFreezableStep left, IFreezableStep compareOperator, IFreezableStep right)
        {
            var dict = new Dictionary<string, IFreezableStep>
            {
                {nameof(Compare<int>.Left), left},
                {nameof(Compare<int>.Operator), compareOperator},
                {nameof(Compare<int>.Right), right},
            };

            var fpd = new FreezableStepData(dict, null, null);
            var step = new CompoundFreezableStep(Instance, fpd, null);

            return step;
        }
    }
}