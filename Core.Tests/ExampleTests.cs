﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests
{

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
[AutoTheory.UseTestOutputHelper]
public partial class ExampleTests
{
    [Theory]
    [Trait("Category", "Integration")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\EntityMapProperties.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\ChangeCase.scl")]
    public async Task RunSCLFromFile(string path)
    {
        var scl = await File.ReadAllTextAsync(path);

        TestOutputHelper.WriteLine(scl);

        var sfs = StepFactoryStore.CreateUsingReflection();

        var stepResult = SCLParsing.ParseSequence(scl).Bind(x => x.TryFreeze(sfs));

        if (stepResult.IsFailure)
            throw new XunitException(
                string.Join(
                    ", ",
                    stepResult.Error.GetAllErrors()
                        .Select(x => x.Message + " " + x.Location.AsString)
                )
            );

        var monad = new StateMonad(
            TestOutputHelper.BuildLogger(),
            SCLSettings.EmptySettings,
            ExternalProcessRunner.Instance,
            FileSystemHelper.Instance,
            sfs
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.ToString()!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunSCLSequence()
    {
        const string scl =
            @"FileRead 'C:\Users\wainw\source\repos\Reductech\ExamplesAndTests\artwork_data.csv'  #Read a file
| FromCSV				#Convert to Array<Entity>
| ArrayFilter ((from <entity> 'artist') == 'Blake, Robert')  # Filter to just entities whose 'artist' property has value 'Blake, Robert'
| EntityMap (in <entity> 'artist' 'Robert Blake') #Set the 'artist' property to 'Robert Blake'
| EntityMapProperties (artist: 'Artist Name' artistId: 'ArtistId') #Rename the 'artist' and 'artistId' properties
| ArraySort (from <entity> 'year') #Sort by the 'year' property
| ArrayDistinct (from <entity> 'id') # Distinct by the 'id' property
| ToJson #Convert to JSON
| FileWrite 'Artwork_Data.json' #Write to a file
";
        //@"FileWrite 'Dinosaur Dinosaur Dinosaur' 'C:\Users\wainw\source\repos\Reductech\core\TestFile.txt' true";
        //            @"GenerateDocumentation | Foreach (
        //- Print (From <Entity> ""FileName"")
        //- Print(From <Entity> ""Title"")
        //- Print(From <Entity> ""Directory"")
        //- Print(From <Entity> ""Category"")
        //)";
        //            @"- <docs> = GenerateDocumentation
        //- <docs> | ArrayDistinct (From <entity> 'Directory') | ForEach (CreateDirectory (PathCombine ['Documentation', (From <Entity> 'Directory')]))
        //- <docs> | Foreach (FileWrite (From <Entity> 'FileText') (PathCombine ['Documentation', (From <Entity> 'Directory'), (From <Entity> 'FileName')]))";

        var logger = TestOutputHelper.BuildLogger(LogLevel.Information);
        var sfs    = StepFactoryStore.CreateUsingReflection();

        var runner = new SCLRunner(
            SCLSettings.EmptySettings,
            logger,
            ExternalProcessRunner.Instance,
            FileSystemHelper.Instance,
            sfs
        );

        var r = await runner.RunSequenceFromTextAsync(scl, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.ToString()!);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunObjectSequence()
    {
        var step = new Sequence<Unit>()
        {
            InitialSteps = new List<IStep<Unit>>
            {
                new SetVariable<Array<Entity>>
                {
                    Variable = new VariableName("EntityStream"),
                    Value = new FromCSV
                    {
                        Stream = new FileRead
                        {
                            Path = new StringConstant(
                                @"C:\Users\wainw\source\repos\Reductech\edr\Examples\Dinosaurs.csv"
                            )
                        }
                    }
                },
                new SetVariable<Entity>()
                {
                    Variable = new VariableName("Schema"),
                    Value = Constant(
                        new Schema
                        {
                            AllowExtraProperties = false,
                            Name                 = "Dinosaur",
                            Properties = new Dictionary<string, SchemaProperty>()
                            {
                                {
                                    "Name",
                                    new SchemaProperty { Type = SchemaPropertyType.String }
                                },
                                {
                                    "ArrayLength",
                                    new SchemaProperty { Type = SchemaPropertyType.Double }
                                },
                                {
                                    "Period",
                                    new SchemaProperty { Type = SchemaPropertyType.String }
                                },
                            }
                        }.ConvertToEntity()
                    )
                },
                new FileWrite
                {
                    Path = new PathCombine { Paths = Array("MyFile.txt") },
                    Stream = new ToCSV
                    {
                        Entities = new EnforceSchema()
                        {
                            EntityStream =
                                new GetVariable<Array<Entity>>()
                                {
                                    Variable =
                                        new VariableName("EntityStream")
                                },
                            Schema = new GetVariable<Entity>()
                            {
                                Variable = new VariableName("Schema")
                            }
                        }
                    }
                }
            },
            FinalStep = new DoNothing()
        };

        var scl = step.Serialize();

        TestOutputHelper.WriteLine(scl);

        var monad = new StateMonad(
            TestOutputHelper.BuildLogger(),
            SCLSettings.EmptySettings,
            ExternalProcessRunner.Instance,
            FileSystemHelper.Instance,
            StepFactoryStore.CreateUsingReflection()
        );

        var r = await (step as IStep<Unit>).Run(monad, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.ToString()!);
    }
}

}
