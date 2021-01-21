﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GenerateDocumentationTests : StepTestBase<GenerateDocumentation, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            static Array<Entity> Entities(params Entity[] entities) => new(entities);

            static Entity File(string fileName, string text)
            {
                return Entity.Create(("FileName", fileName), ("Text", text));
            }

            static Entity Contents(params (string name, string comment)[] steps)
            {
                static string GetNameTerm(string n) => $"[{n}](#{n})";
                var maxNameLength    = Math.Max(4, steps.Max(x => GetNameTerm(x.name).Length));
                var maxCommentLength = Math.Max(7, steps.Max(x => x.comment.Length));

                var nameSpaces    = string.Join("", Enumerable.Repeat(' ', maxNameLength - 4));
                var commentSpaces = string.Join("", Enumerable.Repeat(' ', maxCommentLength - 7));

                var nameDashes    = string.Join("", Enumerable.Repeat('-', maxNameLength - 2));
                var commentDashes = string.Join("", Enumerable.Repeat('-', maxCommentLength - 2));

                var sb = new StringBuilder();
                sb.AppendLine("# Contents");
                sb.AppendLine($"|Step{nameSpaces}|Summary{commentSpaces}|");
                sb.AppendLine($"|:{nameDashes}:|:{commentDashes}:|");

                foreach (var (name, comment) in steps)
                {
                    sb.AppendLine(
                        $"|{GetNameTerm(name).PadRight(maxNameLength)}|{comment.PadRight(maxCommentLength)}|"
                    );
                }

                var text = sb.ToString().Trim();

                return File("Contents", text);
            }

            var not = File(
                "Not",
                "<a name=\"Not\"></a>\r\n## Not\r\n\r\n**Boolean**\r\n\r\nNegation of a boolean value.\r\n\r\n|Parameter|Type  |Required|Summary             |\r\n|:-------:|:----:|:------:|:------------------:|\r\n|Boolean  |`bool`|☑️      |The value to negate.|"
            );

            (string nameof, string comment) notHeader = ("Not", "Negation of a boolean value.");

            var applyMathOperator = File(
                "ApplyMathOperator",
                "<a name=\"ApplyMathOperator\"></a>\r\n## ApplyMathOperator\r\n\r\n**Int32**\r\n\r\nApplies a mathematical operator to two integers. Returns the result of the operation.\r\n\r\n|Parameter|Type                         |Required|Summary               |\r\n|:-------:|:---------------------------:|:------:|:--------------------:|\r\n|Left     |`int`                        |☑️      |The left operand.     |\r\n|Operator |[MathOperator](#MathOperator)|☑️      |The operator to apply.|\r\n|Right    |`int`                        |☑️      |The right operand.    |"
            );

            (string nameof, string comment) mathHeader = (
                "ApplyMathOperator",
                "Applies a mathematical operator to two integers. Returns the result of the operation.");

            (string nameof, string comment) exampleStepHeader = (
                "DocumentationExampleStep",
                "");

            var documentationExample = File(
                "DocumentationExampleStep",
                "<a name=\"DocumentationExampleStep\"></a>\r\n## DocumentationExampleStep\r\n\r\n**StringStream**\r\n\r\n*Requires ValueIf Library Version 1.2*\r\n\r\n|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|\r\n|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|\r\n|Alpha    |`int`                        |☑️      |       |Greater than 1|             |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |\r\n|Beta     |[StringStream](#StringStream)|        |       |              |Two hundred  |       |                 |                 |            |Alpha   |                  |               |\r\n|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |\r\n|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |"
            );

            yield return new StepCase(
                "Generate Not Documentation",
                new GenerateDocumentation(),
                Entities(Contents(notHeader), not)
            ).WithStepFactoryStore(StepFactoryStore.Create(NotStepFactory.Instance));

            yield return new StepCase(
                "Generate Math Documentation",
                new GenerateDocumentation(),
                Entities(
                    Contents(mathHeader),
                    applyMathOperator,
                    File(
                        "MathOperator",
                        "<a name=\"MathOperator\"></a>\r\n## MathOperator\r\nAn operator that can be applied to two numbers.\r\n\r\n|Name    |Summary                                                                                                   |\r\n|:------:|:--------------------------------------------------------------------------------------------------------:|\r\n|None    |Sentinel value                                                                                            |\r\n|Add     |Add the left and right operands.                                                                          |\r\n|Subtract|Subtract the right operand from the left.                                                                 |\r\n|Multiply|Multiply the left and right operands.                                                                     |\r\n|Divide  |Divide the left operand by the right. Attempting to divide by zero will result in an error.               |\r\n|Modulo  |Reduce the left operand modulo the right.                                                                 |\r\n|Power   |Raise the left operand to the power of the right. If the right operand is negative, zero will be returned.|"
                    )
                )
            ).WithStepFactoryStore(StepFactoryStore.Create(ApplyMathOperatorStepFactory.Instance));

            yield return new StepCase(
                "Example step",
                new GenerateDocumentation(),
                Entities(Contents(exampleStepHeader), documentationExample)
            ).WithStepFactoryStore(
                StepFactoryStore.Create(DocumentationExampleStepFactory.Instance)
            );

            yield return new StepCase(
                "Two InitialSteps",
                new GenerateDocumentation(),
                Entities(Contents(notHeader, exampleStepHeader), not, documentationExample)
            ).WithStepFactoryStore(
                StepFactoryStore.Create(
                    NotStepFactory.Instance,
                    DocumentationExampleStepFactory.Instance
                )
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get { yield break; }
    }

    private class
        DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep, StringStream>
    {
        private DocumentationExampleStepFactory() { }

        public static SimpleStepFactory<DocumentationExampleStep, StringStream> Instance { get; } =
            new DocumentationExampleStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Requirement> Requirements
        {
            get
            {
                yield return new Requirement
                {
                    Name = "ValueIf Library", MinVersion = new Version(1, 2)
                };
            }
        }

        /// <inheritdoc />
        public override string Category => "Examples";
    }

    private class DocumentationExampleStep : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        #pragma warning disable 1998
        protected override async Task<Result<StringStream, IError>> Run(
                IStateMonad stateMonad,
                CancellationToken cancellation)
            #pragma warning restore 1998
        {
            throw new Exception("Cannot run Documentation Example Step");
        }

        /// <summary>
        /// The alpha property. Required
        /// </summary>
        [StepProperty(1)]
        [Required]
        [AllowedRange("Greater than 1")]
        [DocumentationURL("alpha.com")]
        [Example("1234")]
        [RecommendedRange("100-300")]
        [RecommendedValue("201")]
        [RequiredVersion("Greek", "2.1")]
        [SeeAlso("Beta")]
        // ReSharper disable UnusedMember.Local
        public IStep<int> Alpha { get; set; } = null!;

        /// <summary>
        /// The beta property. Not Required.
        /// </summary>
        [StepProperty(2)]
        [SeeAlso("Alpha")]
        [DefaultValueExplanation("Two hundred")]
        public IStep<StringStream> Beta { get; set; } = new StringConstant("Two hundred");

        /// <summary>
        /// The delta property.
        /// </summary>
        [StepListProperty(4)]
        [ValueDelimiter(",")]
        public IReadOnlyList<IStep<bool>> Delta { get; set; } = null!;

        /// <summary>
        /// The Gamma property.
        /// </summary>
        [VariableName(3)]
        public VariableName Gamma { get; set; }
        // ReSharper restore UnusedMember.Local

        /// <inheritdoc />
        public override IStepFactory StepFactory => DocumentationExampleStepFactory.Instance;
    }
}

}
