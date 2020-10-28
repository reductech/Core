﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ArrayIsEmptyTests : StepTestBase<ArrayIsEmpty<string>, bool>
    {
        /// <inheritdoc />
        public ArrayIsEmptyTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {

                yield return new DeserializeCase("short form empty",
                    "ArrayIsEmpty(Array = [])",
                    true
                );

                yield return new DeserializeCase("short form",
                    "ArrayIsEmpty(Array = ['Hello','World'])",
                    false
                );

                yield return new DeserializeCase("long form",
                    "Do: ArrayIsEmpty\nArray: ['Hello','World']",
                    false
                );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Empty",
                    new ArrayIsEmpty<string>()
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    }, false);

                yield return new StepCase("Not Empty",
                    new ArrayIsEmpty<string>()
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    }, false);
            }
        }


    }
}