﻿using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SortArrayTests : StepTestBase<SortArray<int>, List<int>>
    {
        /// <inheritdoc />
        public SortArrayTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Ascending", new SortArray<int>()
                {
                    Array = Array(8,6,7,5,3,0,9),
                    Order = Constant(SortOrder.Ascending)

                }, new List<int>(){0,3,5,6,7,8,9} );

                yield return new StepCase("Descending", new SortArray<int>()
                {
                    Array = Array(8, 6, 7, 5, 3, 0, 9),
                    Order = Constant(SortOrder.Descending)

                }, new List<int>() { 9,8,7,6,5,3,0 });
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Sort Ascending",
                    "SortArray(Array = [8,6,7,5,3,0,9], Order = SortOrder.Ascending)",
                    new List<int>(){0,3,5,6,7,8,9});

            }

        }

    }
}