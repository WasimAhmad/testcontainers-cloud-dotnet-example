using System;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TestcontainersCloud.DotNetExample;

[TestClass]
public sealed class TestcontainersCloudFirstTest
{
    //[TestMethod]
    //public async Task TestcontainersCloudDockerEngine()
    //{
    //    using var dockerClientConfiguration =
    //        TestcontainersSettings.OS.DockerEndpointAuthConfig.GetDockerClientConfiguration(ResourceReaper
    //            .DefaultSessionId);

    //    using var dockerClient = dockerClientConfiguration.CreateClient();

    //    var versionResponse = await dockerClient.System.GetSystemInfoAsync()
    //        .ConfigureAwait(false);

    //    var isTestcontainersDesktop = versionResponse.ServerVersion.Contains("Testcontainers Desktop");
    //    var isTestcontainersCloud = versionResponse.ServerVersion.Contains("testcontainerscloud");
    //    if (!(isTestcontainersDesktop || isTestcontainersCloud))
    //    {
    //        Console.WriteLine(PrettyStrings.OhNo);
    //        Assert.Fail();
    //    }

    //    var runtimeName = "Testcontainers Cloud";
    //    if (!versionResponse.ServerVersion.Contains("testcontainerscloud"))
    //    {
    //        runtimeName = versionResponse.OperatingSystem;
    //    }

    //    if (versionResponse.ServerVersion.Contains("Testcontainers Desktop"))
    //    {
    //        runtimeName += " via Testcontainers Desktop app";
    //    }

    //    Console.WriteLine(PrettyStrings.Logo.Replace("::::::", runtimeName));
    //}

    [TestMethod]
    public async Task CreatePostgreSQLContainer()
    {
        const string initScript = """
                                  create table guides (
                                      id         bigserial     not null,
                                      title      varchar(1023)  not null,
                                      url        varchar(1023) not null,
                                      primary key (id)
                                  );
                                  insert into guides(title, url)
                                  values ('Getting started with Testcontainers', 'https://testcontainers.com/getting-started/'),
                                         ('Getting started with Testcontainers for Java', 'https://testcontainers.com/guides/getting-started-with-testcontainers-for-java/'),
                                         ('Getting started with Testcontainers for .NET', 'https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/'),
                                         ('Getting started with Testcontainers for Node.js', 'https://testcontainers.com/guides/getting-started-with-testcontainers-for-nodejs/'),
                                         ('Getting started with Testcontainers for Go', 'https://testcontainers.com/guides/getting-started-with-testcontainers-for-go/'),
                                         ('Testcontainers container lifecycle management using JUnit 5', 'https://testcontainers.com/guides/testcontainers-container-lifecycle/');
                                  """;

        await using var postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14-alpine")
            .WithResourceMapping(Encoding.Default.GetBytes(initScript), "/docker-entrypoint-initdb.d/init.sql")
            .Build();

        await postgreSqlContainer.StartAsync()
            .ConfigureAwait(false);
        
        await using var dataSource = NpgsqlDataSource.Create(postgreSqlContainer.GetConnectionString());
        await using var command = dataSource.CreateCommand("SELECT COUNT(*) FROM guides");
        var count = (Int64) command.ExecuteScalar();
        Assert.AreEqual(6, count);
    }
}