﻿using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class RepeatTests : StepTestBase<Repeat<int>, List<int>>
    {
        /// <inheritdoc />
        public RepeatTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Repeat number", new Repeat<int>()
                {
                    Element = Constant(6),
                    Number = Constant(3)
                }, new List<int>(){6,6,6} );


                yield return new StepCase("Repeat zero times", new Repeat<int>()
                {
                    Element = Constant(6),
                    Number = Constant(0)
                }, new List<int>());
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Repeat number", "Repeat(Element = 6, X = 3)", new List<int>(){6,6,6});
            }

        }

    }
}