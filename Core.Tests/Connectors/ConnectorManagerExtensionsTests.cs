﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Packaging;
using Reductech.EDR.ConnectorManagement;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Connectors;
using Xunit;

namespace Reductech.EDR.Core.Tests.Connectors
{

public class FakeConnectorRegistry : IConnectorRegistry
{
    public Task<ICollection<ConnectorMetadata>> Find(
        string search,
        bool prerelease = false,
        CancellationToken ct = default) => throw new NotImplementedException();

    public Task<bool> Exists(
        string id,
        string? version = null,
        CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ICollection<string>> GetVersion(
        string id,
        bool prerelease = false,
        CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ConnectorPackage> GetConnectorPackage(
        string id,
        string version,
        CancellationToken ct = default) => Task.FromResult(
        new ConnectorPackage(
            new ConnectorMetadata(id, version),
            new PackageArchiveReader(
                File.OpenRead(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Connectors",
                        "reductech.edr.connectors.filesystem.0.9.0.nupkg"
                    )
                )
            )
        )
    );
}

public class TestConnectorManager : ConnectorManager
{
    public TestConnectorManager(
        ILogger<ConnectorManager> logger,
        ConnectorManagerSettings settings,
        IConnectorRegistry registry,
        IConnectorConfiguration configuration,
        IFileSystem fileSystem) : base(
        logger,
        settings,
        registry,
        configuration,
        fileSystem
    ) { }

    protected override Assembly LoadPlugin(string dllPath, ILogger logger) => Mock.Of<Assembly>();
}

public class ConnectorManagerExtensionsTests
{
    private static Dictionary<string, ConnectorSettings> DefaultConnectors => new()
    {
        {
            "Reductech.EDR.Connectors.Nuix",
            new ConnectorSettings { Id = "Reductech.EDR.Connectors.Nuix", Version = "0.9.0" }
        },
        {
            "Reductech.EDR.Connectors.StructuredData",
            new ConnectorSettings
            {
                Id = "Reductech.EDR.Connectors.StructuredData", Version = "0.9.0"
            }
        }
    };

    private readonly ILoggerFactory _loggerFactory;
    private readonly IConnectorConfiguration _config;
    private readonly ConnectorManagerSettings _settings;
    private readonly MockFileSystem _fileSystem;
    private readonly IConnectorRegistry _registry;
    private readonly ConnectorManager _manager;

    public ConnectorManagerExtensionsTests()
    {
        _loggerFactory = TestLoggerFactory.Create();
        _config        = new ConnectorConfiguration(DefaultConnectors);
        _settings      = ConnectorManagerSettings.Default;
        _fileSystem    = new MockFileSystem();
        _registry      = new FakeConnectorRegistry();

        _manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );
    }

    [Fact]
    public async Task GetStepFactoryStoreAsync_ReturnsStepFactory()
    {
        var sfs = await _manager.GetStepFactoryStoreAsync();
        Assert.NotNull(sfs);
    }

    [Fact]
    public async Task GetStepFactoryStoreAsync_WhenValidationFails_Throws()
    {
        var manager = new ConnectorManager(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings with { AutoDownload = false },
            _registry,
            _config,
            _fileSystem
        );

        var error =
            await Assert.ThrowsAsync<ConnectorManagerExtensions.ConnectorConfigurationException>(
                () => manager.GetStepFactoryStoreAsync()
            );

        Assert.Equal("Could not validate installed connectors.", error.Message);
    }

    [Fact]
    public async Task GetStepFactoryStoreAsync_WhenSame_Throws()
    {
        await _config.AddAsync(
            "Reductech.EDR.Connectors.StructuredData -old",
            new ConnectorSettings
            {
                Id = "Reductech.EDR.Connectors.StructuredData", Version = "0.8.0", Enable = true
            }
        );

        var mock = new Mock<TestConnectorManager>(
            _loggerFactory.CreateLogger<ConnectorManager>(),
            _settings,
            _registry,
            _config,
            _fileSystem
        );

        var error =
            await Assert.ThrowsAsync<ConnectorManagerExtensions.ConnectorConfigurationException>(
                () => mock.Object.GetStepFactoryStoreAsync()
            );

        Assert.Equal(
            "When using multiple configurations with the same connector id, at most one can be enabled.",
            error.Message
        );
    }
}

}
