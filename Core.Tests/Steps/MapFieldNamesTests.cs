﻿using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class MapFieldNamesTests : StepTestBase<MapFieldNames, EntityStream>
    {
        /// <inheritdoc />
        public MapFieldNamesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new SequenceStepCase("Map some fields",

                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>()
                        {
                            new ForEachEntity
                            {
                                VariableName = new VariableName("Foo"),
                                Action = new Print<Entity>
                                {
                                    Value = GetVariable<Entity>("Foo")
                                },
                                EntityStream =
                                    new MapFieldNames
                                    {
                                        EntityStream = Constant(EntityStream.Create(
                                            CreateEntity(new KeyValuePair<string, string>("Food", "Hello"),
                                                new KeyValuePair<string, string>("Bar", "World")),
                                            CreateEntity(new KeyValuePair<string, string>("Food", "Hello 2"),
                                                new KeyValuePair<string, string>("Bar", "World 2")))),

                                        Mappings = new Constant<Entity>(CreateEntity(new KeyValuePair<string, string>("Food", "Foo")))
                                    }
                            }

                        }
                    }, "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                ).WithExpectedFinalState("Foo",
                    CreateEntity(new KeyValuePair<string, string>("Foo", "Hello 2"),
                        new KeyValuePair<string, string>("Bar", "World 2")));
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var step = CreateStepWithDefaultOrArbitraryValues();

                yield return new SerializeCase("default", step.step,
                    @"Do: MapFieldNames
EntityStream:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')
Mappings: (Prop1 = 'Val6',Prop2 = 'Val7')");


            }
        }
    }
}