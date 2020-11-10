﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;


namespace Reductech.EDR.Core.Tests
{
    public class DeserializationTests : DeserializationTestCases
    {
        public DeserializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DeserializationTestCases))]
        public override Task Test(string key) => base.Test(key);
    }

    public class DeserializationTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                yield return new DeserializationTestFunction(
                    @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print(Value = <Bar>)", "Hello World");

                yield return new DeserializationTestFunction(@"Print(Value = 'Mark''s string')", "Mark's string" );
                yield return new DeserializationTestFunction(@"Print(Value = ""Mark's string"")", "Mark's string" );


                yield return new DeserializationTestFunction(@"Print(Value = 2 * 3)", 6);

                yield return new DeserializationTestFunction(@"Print(Value = 3 - 2)", 1);

                yield return new DeserializationTestFunction(@"print(value = 2 * 3)", 6);

                yield return new DeserializationTestFunction(@"print(value = 6 / 3)", 2);

                yield return new DeserializationTestFunction(@"print(value = 6 ^ 2)", 36);

                yield return new DeserializationTestFunction(@"print(value = 7 % 2)", 1);

                yield return new DeserializationTestFunction(@"print(value = 7 modulo 2)", 1);

                yield return new DeserializationTestFunction(@"do: Print
Value: falsetto", "falsetto"); //check 'false' delimiter

                yield return new DeserializationTestFunction(@"do: Print
Value: notable", "notable");//check 'not' delimiter

                yield return new DeserializationTestFunction(@"print(value=2*3)", 6);

                yield return new DeserializationTestFunction(@"Print(Value = 2 ^ 3)", 8);

                yield return new DeserializationTestFunction(@"Print(Value = not (True))", false);


                yield return new DeserializationTestFunction(@"Print(Value = 2 >= 3)", false);
                yield return new DeserializationTestFunction(@"Print(Value = 4 >= 3)", true);
                yield return new DeserializationTestFunction(@"Print(Value = 3 >= 3)", true);

                yield return new DeserializationTestFunction(@"Print(Value = 3 > 3)", false);
                yield return new DeserializationTestFunction(@"Print(Value = 4 > 3)", true);
                yield return new DeserializationTestFunction(@"Print(Value = 3 < 3)", false);

                yield return new DeserializationTestFunction(@"Print(Value = 3 <= 3)", true);

                yield return new DeserializationTestFunction(@"Print(Value = 2 * (3 + 4))",14);
                yield return new DeserializationTestFunction(@"Print(Value = (2 * 3) + 4)",10);

                yield return new DeserializationTestFunction(@"Print(Value = (2 >= 3))", false);

                yield return new DeserializationTestFunction(@"Print(Value = (2 * (3 + 4)))", 14);
                yield return new DeserializationTestFunction(@"Print(Value = (2*(3+4)))", 14);
                yield return new DeserializationTestFunction(@"Print(Value = ((2 * 3) + 4))", 10);

                yield return new DeserializationTestFunction(@"Print(Value = True && False)", false);

                yield return new DeserializationTestFunction(@"Print(Value = StringIsEmpty(String = 'Hello') && StringIsEmpty(String = 'World'))", false);

                yield return new DeserializationTestFunction(@"Print(Value = not (True) && not(False))", false);

                yield return new DeserializationTestFunction(@"Print(Value = true && false)", false);

                yield return new DeserializationTestFunction(@"Print(Value = true and false)", false);

                yield return new DeserializationTestFunction("Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))", true);

                yield return new DeserializationTestFunction(@"<ArrayVar> = Array(Elements = ['abc', '123'])");

                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', '123'])
- Print(Value = ArrayCount(Array = <ArrayVar>))",2);

                yield return new DeserializationTestFunction(@"Print(Value = ArrayCount(Array = ['abc', '123']))", 2);

                yield return new DeserializationTestFunction(@"
- <ArrayVar> =  ['abc', '123']
- Print(Value = ArrayCount(Array = <ArrayVar>))", 2);





                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', '123'])
- Print(Value = ArrayIsEmpty(Array = <ArrayVar>))",false);



                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', '123'])
- Print(Value = ElementAtIndex(Array = <ArrayVar>, Index = 1))", "123");

                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', '123'])
- Print(Value = FirstIndexOfElement(Array = <ArrayVar>, Element = '123'))", "1");

                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', '123'])
- Foreach(Array = <ArrayVar>, VariableName = <Element>, Action = Print(Value = <Element>))", "abc", "123");


                yield return new DeserializationTestFunction(@"
- <ArrayVar1> = Array(Elements = ['abc', '123'])
- <ArrayVar2> = Repeat(Element = <ArrayVar1>, Number = 2)
- Foreach(Array = <ArrayVar2>, VariableName = <Element>, Action = Print(Value = ArrayCount(Array = <Element>)))", "2", "2");

                yield return new DeserializationTestFunction(@"
- <ArrayVar> = Array(Elements = ['abc', 'def'])
- <Sorted> = SortArray(Array = <ArrayVar>)
- Print(Value = ElementAtIndex(Array = <Sorted>, Index = 0))", "abc");

                yield return new DeserializationTestFunction(@"
- <ConditionVar> = true
- Conditional(Condition = <ConditionVar>, ThenStep = Print(Value = 1), ElseStep = Print(Value = 2))", "1");

                yield return new DeserializationTestFunction(
                    @"Do: Print
Config:
  AdditionalRequirements: 
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config", "I have config"
                )
                {
                    ExpectedConfiguration = new Configuration
                    {
                        TargetMachineTags = new List<string> {"Tag1"},
                        DoNotSplit = false,
                        Priority = 1
                    }
                };

                yield return new DeserializationTestFunction(@"Do: Print
Config:
  AdditionalRequirements:
  - Notes: ABC123
    Name: Test
    MinVersion: 1.2.3.4
    MaxVersion: 5.6.7.8
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config too", "I have config too")
                {
                    ExpectedConfiguration = new Configuration
                    {
                        TargetMachineTags = new List<string> { "Tag1" },
                        DoNotSplit = false,
                        Priority = 1,
                        AdditionalRequirements = new List<Requirement>
                        {
                            new Requirement
                            {
                                MaxVersion = new Version(5,6,7,8),
                                MinVersion = new Version(1,2,3,4),
                                Name = "Test",
                                Notes = "ABC123"
                            }
                        }
                    }
                };


//                yield return new DeserializationTestFunction(@"Do: Print
//Value:
//  Do: ElementAtIndex
//  Array:
//    Do: ElementAtIndex
//    Array:
//      Do: ReadCsv
//      Text: >-
//        Name,Summary

//        One,The first number

//        Two,The second number
//      ColumnsToMap: Array(Elements = ['Name', 'Summary'])
//    Index: 1
//  Index: 1", "The second number");


//                yield return new DeserializationTestFunction(@"- Do: SetVariable
//  VariableName: <TextVar>
//  Value: >-
//    Name,Summary

//    One,The first number

//    Two,The second number
//- <CSVVar> = ReadCsv(ColumnsToMap = Array(Elements = ['Name', 'Summary']), Delimiter = ',', HasFieldsEnclosedInQuotes = False, Text = <TextVar>)
//- ForEach(Action = Print(Value = ElementAtIndex(Array = <Foo>, Index = 0)), Array = <CSVVar>, VariableName = <Foo>)",
//                    "One", "Two"
//                    );




//                yield return new DeserializationTestFunction(@"- Do: SetVariable
//  VariableName: <TextVar>
//  Value: >-
//    Name,Summary

//    One,The first number

//    Two,The second number
//- <SearchTerms> = ReadCsv(ColumnsToMap = Array(Elements = ['Name', 'Summary']), Delimiter = ',', HasFieldsEnclosedInQuotes = False, Text = <TextVar>)
//- Do: ForEach
//  Array: <SearchTerms>
//  VariableName: <Row>
//  Action: Print(Value = ElementAtIndex(Array = <Row>, Index = 0))",
//                    "One", "Two");




                yield return new DeserializationTestFunction(@"ForEach(Array = ['a','b','c'], VariableName = <char>, Action = Print(Value = <char>))", "a", "b", "c");
                yield return new DeserializationTestFunction(@"ForEach(
Array = ['a','b','c'],
VariableName = <char>,
Action = Print(Value = <char>))", "a", "b", "c");


            }
        }


        private class DeserializationTestFunction : ITestBaseCaseParallel
        {

            public DeserializationTestFunction(string yaml, params object[] expectedLoggedValues)
            {
                Yaml = yaml;
                ExpectedLoggedValues = expectedLoggedValues.Select(x => x.ToString()!).ToList();
            }

            /// <inheritdoc />
            public string Name => Yaml;

            private string Yaml { get; }

            public Configuration? ExpectedConfiguration { get; set; } = null!;

            private IReadOnlyCollection<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                testOutputHelper.WriteLine(Yaml);

                var stepFactoryStore = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
                var logger = new TestLogger();

                var deserializeResult = YamlMethods.DeserializeFromYaml(Yaml, stepFactoryStore);

                deserializeResult.ShouldBeSuccessful(x=>x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze();

                freezeResult.ShouldBeSuccessful(x=>x.AsString);


                var unitStep = freezeResult.Value as IStep<Unit>;

                unitStep.Should().NotBeNull();

                var runResult = await unitStep!
                    .Run(new StateMonad(logger, EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance,  stepFactoryStore), CancellationToken.None);

                runResult.ShouldBeSuccessful(x => x.AsString);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);


                if (ExpectedConfiguration != null || freezeResult.Value.Configuration != null)
                {
                    freezeResult.Value.Configuration.Should().BeEquivalentTo(ExpectedConfiguration);
                }
            }
        }
    }
}