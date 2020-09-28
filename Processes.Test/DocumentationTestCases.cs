﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Internal.Documentation;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class DocumentationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new DocumentationTestCase(
                    new List<IStepFactory> {NotStepFactory.Instance},
                    "# Contents",
                    "|<a name=\"Not\">Not</a>|Negation of a boolean value.|",
                    "|:-------------------:|:--------------------------:|",
                    "# Processes",
                    "<a name=\"Not\"></a>",
                    "## Not",
                    "**Boolean**",
                    "Negation of a boolean value.",
                    "|Parameter|Type  |Required|Summary             |",
                    "|:-------:|:----:|:------:|:------------------:|",
                    "|Boolean  |`bool`|☑️      |The value to negate.|");
            }
        }
        private class DocumentationTestCase : ITestCase
        {
            /// <summary>
            /// Create a new DocumentationTestCase
            /// </summary>
            public DocumentationTestCase(
                IReadOnlyCollection<IStepFactory> factories,
                params string[] expectedLines)
            {
                ExpectedLines = expectedLines;
                Factories = factories;
                Name = string.Join(" ", Factories.Select(x => x.TypeName).OrderBy(x=>x));
            }

            /// <inheritdoc />
            public string Name { get; }

            public IReadOnlyCollection<string> ExpectedLines { get; }

            public IReadOnlyCollection<IStepFactory> Factories { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var documented = Factories.Select(x => new StepWrapper(x, new DocumentationCategory("Processes")));

                var lines = DocumentationCreator.CreateDocumentationLines(documented);

                foreach (var line in lines)
                {
                    testOutputHelper.WriteLine(line);
                }

                lines.Where(x=>!string.IsNullOrWhiteSpace(x)).Should().BeEquivalentTo(ExpectedLines);
            }
        }
    }
}