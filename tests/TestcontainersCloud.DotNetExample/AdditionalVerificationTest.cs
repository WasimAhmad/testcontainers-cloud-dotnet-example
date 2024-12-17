using System;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TestcontainersCloud.DotNetExample
{
    [TestClass]
    public sealed class AdditionalVerificationTest
    {
        [TestMethod]
        public async Task VerifySpecificGuideEntry()
        {
            // SQL script to create the table and insert a specific guide
            const string initScript = """
                create table guides (
                    id         bigserial     not null,
                    title      varchar(1023) not null,
                    url        varchar(1023) not null,
                    primary key (id)
                );
                
                insert into guides(title, url)
                values ('Getting started with Testcontainers for .NET', 'https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/');
            """;

            // Build and start a new PostgreSQL test container
            await using var postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:14-alpine")
                .WithResourceMapping(Encoding.Default.GetBytes(initScript), "/docker-entrypoint-initdb.d/init.sql")
                .Build();

            await postgreSqlContainer.StartAsync().ConfigureAwait(false);

            // Connect to the database and query for the inserted guide
            await using var dataSource = NpgsqlDataSource.Create(postgreSqlContainer.GetConnectionString());
            await using var command = dataSource.CreateCommand(
                "SELECT url FROM guides WHERE title = 'Getting started with Testcontainers for .NET';"
            );

            var url = (string?)command.ExecuteScalar();

            // Verify that the URL matches the inserted value
            Assert.IsNotNull(url, "Expected guide was not found in the database.");
            Assert.AreEqual("https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/", url);
        }
    }
}
