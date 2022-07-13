using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Hackney.Core.Sns;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AssetInformationApi.Tests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef
            {
                Name = "Assets",
                KeyName = "id",
                KeyType = ScalarAttributeType.S,
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(new[]
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "AssetParentsAndChilds",
                        KeySchema = new List<KeySchemaElement>(new[]
                        {
                            new KeySchemaElement("rootAsset", KeyType.HASH),
                            new KeySchemaElement("parentAssetIds", KeyType.RANGE)
                        }),
                        Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        ProvisionedThroughput = new ProvisionedThroughput(10 , 10)
                    },
                    new GlobalSecondaryIndex
                    {
                        IndexName = "AssetId",
                        KeySchema = new List<KeySchemaElement>(new[]
                        {
                            new KeySchemaElement("assetId", KeyType.HASH)
                        }),
                        Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        ProvisionedThroughput = new ProvisionedThroughput(10 , 10)
                    }
                })
            }
        };

        public IDynamoDbFixture DynamoDbFixture { get; private set; }
        public HttpClient Client { get; private set; }
        public ISnsFixture SnsFixture { get; private set; }

        public MockWebApplicationFactory()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");

            EnsureEnvVarConfigured("Sns_LocalMode", "true");
            EnsureEnvVarConfigured("Localstack_SnsServiceUrl", "http://localhost:4566");

            Client = CreateClient();
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureDynamoDB();
                services.ConfigureDynamoDbFixture();

                services.ConfigureSns();
                services.ConfigureSnsFixture();

                var serviceProvider = services.BuildServiceProvider();

                DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
                DynamoDbFixture.EnsureTablesExist(_tables);

                SnsFixture = serviceProvider.GetRequiredService<ISnsFixture>();
                SnsFixture.CreateSnsTopic<EntityEventSns>("asset.fifo", "ASSET_SNS_ARN");
            });
        }
    }
}
