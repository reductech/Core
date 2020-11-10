﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class CompareTests : StepTestBase<Compare<int>, bool>
    {
        /// <inheritdoc />
        public CompareTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                var cases = new List<(int left, int right, CompareOperator op, bool expectedOutput)>
               {
                   (1,1, CompareOperator.Equals, true),
                   (1,2, CompareOperator.Equals, false),

                   (1,1, CompareOperator.NotEquals, false),
                   (1,2, CompareOperator.NotEquals, true),

                   (1,2, CompareOperator.LessThan, true),
                   (2,2, CompareOperator.LessThan, false),

                   (3,2, CompareOperator.LessThanOrEqual, false),
                   (2,2, CompareOperator.LessThanOrEqual, true),

                   (2,1, CompareOperator.GreaterThan, true),
                   (2,2, CompareOperator.GreaterThan, false),

                   (2,3, CompareOperator.GreaterThanOrEqual, false),
                   (2,2, CompareOperator.GreaterThanOrEqual, true),
               };

                foreach (var (left, right, op, expectedOutput) in cases)
                {
                    yield return new DeserializeCase($"{left} {op} {right}",$"{left} {op} {right}" , expectedOutput);
                }
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
               var cases = new List<(int left, int right, CompareOperator op, bool expectedOutput)>
               {
                   (1,1, CompareOperator.Equals, true),
                   (1,2, CompareOperator.Equals, false),

                   (1,1, CompareOperator.NotEquals, false),
                   (1,2, CompareOperator.NotEquals, true),

                   (1,2, CompareOperator.LessThan, true),
                   (2,2, CompareOperator.LessThan, false),

                   (3,2, CompareOperator.LessThanOrEqual, false),
                   (2,2, CompareOperator.LessThanOrEqual, true),

                   (2,1, CompareOperator.GreaterThan, true),
                   (2,2, CompareOperator.GreaterThan, false),

                   (2,3, CompareOperator.GreaterThanOrEqual, false),
                   (2,2, CompareOperator.GreaterThanOrEqual, true),
               };

               foreach (var (left, right, op, expectedOutput) in cases)
               {
                   yield return new StepCase($"{left} {op} {right}", new Compare<int>
                   {
                       Left = Constant(left),
                       Right = Constant(right),
                       Operator = Constant(op)
                   }, expectedOutput );
               }
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Operator None",
                    new Compare<int>
                    {
                        Left = Constant(1),
                        Right = Constant(1),
                        Operator = Constant(CompareOperator.None)
                    },
                    new ErrorBuilder($"Could not apply '{CompareOperator.None}'", ErrorCode.UnexpectedEnumValue)
                );

                yield return CreateDefaultErrorCase();
            } }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return new SerializeCase("Compare",
                    new Compare<int>
                    {
                        Left = Constant(1),
                        Right = Constant(2),
                        Operator = Constant(CompareOperator.LessThan)
                    }, "(1 < 2)"
                );
            } }
    }
}