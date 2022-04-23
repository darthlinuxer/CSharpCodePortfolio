using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using RavenConnection.Models;
using System;
using System.Security.Cryptography.X509Certificates;

namespace RavenConnection.Database
{
    public class DocumentStoreHolder : IDocumentStoreHolder
    {
        private readonly ILogger<DocumentStoreHolder> _logger;
        private RavenConfiguration RavenDbConfig { get; }
        public IDocumentStore Store { get; }

        /// <summary>
        /// Raven Document Store
        /// </summary>
        /// <param name="raven"></param>
        /// <param name="logger"></param>
        public DocumentStoreHolder(
            IOptions<RavenConfiguration> raven,
            ILogger<DocumentStoreHolder> logger)
        {
            this._logger = logger;
            this.RavenDbConfig = raven.Value;

            Store = CreateStore();
            CreateDatabaseIfNotExists();

            this._logger.LogInformation(
                "🌟  Initialized RavenDB document store for {0} at {1}",
                RavenDbConfig.Database, RavenDbConfig.UrlFromContainer);

            // Create indexes
            // IndexCreation.CreateIndexes(
            //     typeof(Talks_BySpeaker).Assembly, Store);
        }

        private IDocumentStore CreateStore()
        {
            bool runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            string serverUrl = runningInContainer ? RavenDbConfig.UrlFromContainer : RavenDbConfig.UrlFromWindows;

            /*bool ravenIsSecure = Environment.GetEnvironmentVariable("RAVEN_IS_SECURE") == "true";
            
            if (ravenIsSecure)
            {
                string ravenServerPassword = Environment.GetEnvironmentVariable("RAVEN_Security_Certificate_Password");
                string certificateFilePath = "/app/certs/server.pfx";
                try
                {
                    return new DocumentStore()
                    {
                        Urls = new[] { serverUrl },
                        Conventions =
                        {
                            MaxNumberOfRequestsPerSession = 10,
                            UseOptimisticConcurrency = true
                        },
                        Database = RavenDbConfig.Database,
                        Certificate = new X509Certificate2(
                            fileName: certificateFilePath)
                    }.Initialize();
                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message} , {ex.InnerException?.Message}");
                }
            }*/

            return new DocumentStore()
            {
                Urls = new[]{serverUrl},
                Conventions =
                {
                    MaxNumberOfRequestsPerSession = 10,
                    UseOptimisticConcurrency = true
                },
                Database = RavenDbConfig.Database
            }.Initialize();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                DatabaseRecord databaseRecord = new DatabaseRecord(RavenDbConfig.Database);
                CreateDatabaseOperation createDatabaseOperation = new CreateDatabaseOperation(databaseRecord);

                Store.Maintenance.Server.Send(createDatabaseOperation);
            }
            catch (ConcurrencyException)
            {
                // Database already exists, do nothing
            }
        }
    }
}
