﻿using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ReadConcordanceTests : StepTestBase<FromConcordance, EntityStream>
    {
        /// <inheritdoc />
        public ReadConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Read Concordance and print all lines",
                    new EntityForEach
                    {
                        EntityStream = new FromConcordance()
                        {
                            Stream = new StringToStream
                            {
                                String = Constant(
                                    $@"þFooþþBarþ{Environment.NewLine}þHelloþþWorldþ{Environment.NewLine}þHello 2þþWorld 2þ")
                            }
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity>() { Variable = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2");

                yield return new StepCase("Read Concordance with multiValue and print all lines",
                    new EntityForEach
                    {
                        EntityStream = new FromConcordance
                        {
                            Stream = new StringToStream
                            {
                                String = Constant(
                                    $@"þFooþþBarþ{Environment.NewLine}þHelloþþWorld|Earthþ{Environment.NewLine}þHello 2þþWorld 2|Earth 2þ")
                            }
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity> { Variable = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "Foo: Hello, Bar: World, Earth",
                    "Foo: Hello 2, Bar: World 2, Earth 2");


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var (step, _) = CreateStepWithDefaultOrArbitraryValues();

                const string expectedYaml = @"Do: FromConcordance
Stream: 'Baz0'
Encoding: EncodingEnum.UTF8
Delimiter: ""\x14""
QuoteCharacter: 'þ'
MultiValueDelimiter: '|'";



                yield return new SerializeCase("Default", step, expectedYaml);
            }
        }

    }
}