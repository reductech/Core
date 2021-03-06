﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using CSharpFunctionalExtensions;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

public partial class PropertyRequirementTests
{
    [GenerateTheory("RequirementTests")]
    private IEnumerable<TestCase> TestCases
    {
        get
        {
            var placeholder = new BoolConstant(true);

            yield return new TestCase(
                "No Requirement",
                new RequirementTestStep(),
                null,
                true
            );

            yield return new TestCase(
                "Requirement not met",
                new RequirementTestStep { RequirementStep = placeholder },
                null,
                false
            );

            yield return new TestCase(
                "Requirement met",
                new RequirementTestStep { RequirementStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                true
            );

            yield return new TestCase(
                "Version below min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                false
            );

            yield return new TestCase(
                "Version above min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                CreateWidgetSettings(new Version(3, 0)),
                true
            );

            yield return new TestCase(
                "Version above max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                CreateWidgetSettings(new Version(6, 0)),
                false
            );

            yield return new TestCase(
                "Version below max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                CreateWidgetSettings(new Version(4, 0)),
                true
            );

            yield return new TestCase(
                "All requirements met",
                new RequirementTestStep
                {
                    MaxVersionStep  = placeholder,
                    MinVersionStep  = placeholder,
                    RequirementStep = placeholder
                },
                CreateWidgetSettings(new Version(5, 0)),
                true
            );

            yield return new TestCase(
                "No Features",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                false
            );

            yield return new TestCase(
                "Wrong Feature",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0), "Kludge"),
                false
            );

            yield return new TestCase(
                "Right Features",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0), "sprocket"),
                true
            );
        }
    }

    private record TestCase(
        string Name,
        RequirementTestStep Step,
        ConnectorSettings? ConnectorSettings,
        bool ExpectSuccess) : ITestInstance
    {
        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            ConnectorData[] connectorData;

            if (ConnectorSettings is null)
                connectorData = Array.Empty<ConnectorData>();
            else
            {
                connectorData = new[] { new ConnectorData(ConnectorSettings, null) };
            }

            var sfs = StepFactoryStore.Create(connectorData);

            var r = Step.Verify(sfs);

            if (ExpectSuccess)
                r.ShouldBeSuccessful();
            else
                r.ShouldBeFailure();
        }
    }

    private static ConnectorSettings CreateWidgetSettings(Version version, params string[] features)
    {
        var connectorSettings =
            new ConnectorSettings
            {
                Id      = "Reductech.EDR.Core.Tests",
                Version = new Version(1, 0).ToString(),
                Enable  = true,
                Settings =
                    new Dictionary<string, object>
                    {
                        { RequirementTestStep.VersionKey, version },
                        { RequirementTestStep.FeaturesKey, features },
                    }
            };

        return connectorSettings;
    }

    private class RequirementTestStep : CompoundStep<bool>
    {
        public const string VersionKey = "WidgetVersion";
        public const string FeaturesKey = "WidgetFeatures";

        /// <inheritdoc />
        protected override async Task<Result<bool, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }

        [StepProperty(1)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, null)]
        public IStep<bool>? RequirementStep { get; init; }

        [StepProperty(2)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, "2.0")]
        public IStep<bool>? MinVersionStep { get; init; }

        [StepProperty(3)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, null, "5.0")]
        public IStep<bool>? MaxVersionStep { get; init; }

        [StepProperty(4)]
        [DefaultValueExplanation("Nothing")]
        [RequiredFeature(FeaturesKey, "sprocket")]
        public IStep<bool>? RequiredFeatureStep { get; init; }

        /// <inheritdoc />
        public override IStepFactory StepFactory => RequirementTestStepFactory.Instance;
    }

    private class RequirementTestStepFactory : SimpleStepFactory<RequirementTestStep, bool>
    {
        private RequirementTestStepFactory() { }

        public static SimpleStepFactory<RequirementTestStep, bool> Instance { get; } =
            new RequirementTestStepFactory();
    }
}

}
