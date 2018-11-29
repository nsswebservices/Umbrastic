using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbrastic.Core.Config;
using Umbrastic.Core.Indexing.Content;
using Umbrastic.Core.Management;
using Umbrastic.Core.Indexing.Media;

namespace Umbrastic.Core.Utils
{
    public static class UmbracoSearchFactory
    {
        private static IElasticClient _client;

        private static readonly IDictionary<IContentIndexService<IContent>, Func<IContent, bool>> ContentIndexServiceRegistry = new Dictionary<IContentIndexService<IContent>, Func<IContent, bool>>();
        private static readonly IDictionary<IMediaIndexService<IMedia>, Func<IMedia, bool>> MediaIndexServiceRegistry = new Dictionary<IMediaIndexService<IMedia>, Func<IMedia, bool>>();

        private static IIndexCreator _indexStrategy;

        public static IEnumerable<IContentIndexService<IContent>> GetContentIndexServices()
        {
            return ContentIndexServiceRegistry.Keys;
        }

        public static IEnumerable<IMediaIndexService<IMedia>> GetMediaIndexServices()
        {
            return MediaIndexServiceRegistry.Keys;
        }

        public static void RegisterIndexStrategy(IIndexCreator strategy)
        {
            _indexStrategy = strategy;
            LogHelper.Info<IIndexCreator>($"Registered index strategy [{strategy.GetType().Name}]");
        }

        public static void RegisterContentIndexService<TIndexService>(TIndexService indexService, Func<IContent, bool> resolver) where TIndexService : IContentIndexService<IContent>
        {
            if (!ContentIndexServiceRegistry.ContainsKey(indexService))
            {
                LogHelper.Info<TIndexService>(() => $"Registered content index service for [{indexService.GetType().Name}]");
                ContentIndexServiceRegistry.Add(indexService, resolver);
            }
            else
            {
                LogHelper.Warn<TIndexService>($"Registration for content index service [{indexService.GetType().Name}] already exists");
            }
        }

        public static void RegisterMediaIndexService<TIndexService>(TIndexService indexService, Func<IMedia, bool> resolver) where TIndexService : IMediaIndexService<IMedia>
        {
            if (!MediaIndexServiceRegistry.ContainsKey(indexService))
            {
                LogHelper.Info<TIndexService>(() => $"Registered media index service for [{indexService.GetType().Name}]");
                MediaIndexServiceRegistry.Add(indexService, resolver);
            }
            else
            {
                LogHelper.Warn<TIndexService>($"Registration for media index service [{indexService.GetType().Name}] already exists");
            }
        }

        public static IIndexCreator GetIndexStrategy()
        {
            return _indexStrategy;
        }

        public static IMediaIndexService<IMedia> GetMediaIndexService(IMedia media)
        {
            return MediaIndexServiceRegistry?.FirstOrDefault(x => x.Value(media)).Key;
        }

        public static IMediaIndexService<IMedia> GetMediaIndexService(Func<IMediaIndexService<IMedia>, bool> predicate)
        {
            return MediaIndexServiceRegistry?.Keys.FirstOrDefault(predicate);
        }

        public static IMediaIndexService<IMedia> GetMediaIndexService(string documentTypeName)
        {
            return MediaIndexServiceRegistry?.Keys.FirstOrDefault(x => x.DocumentTypeName.Equals(documentTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static IContentIndexService<IContent> GetContentIndexService(IContent content)
        {
            return ContentIndexServiceRegistry?.FirstOrDefault(x => x.Value(content)).Key;
        }

        public static IContentIndexService<IContent> GetContentIndexService(Func<IContentIndexService<IContent>, bool> predicate)
        {
            return ContentIndexServiceRegistry?.Keys.FirstOrDefault(predicate);
        }
        public static IContentIndexService<IContent> GetContentIndexService(string documentTypeName)
        {
            return ContentIndexServiceRegistry?.Keys.FirstOrDefault(x => x.DocumentTypeName.Equals(documentTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static IElasticClient Client
        {
            get
            {
                if (_client == null) throw new InvalidOperationException("Elasticsearch client is not available, verify configuration settings");
                return _client;
            }
        }

        public static void SetDefaultClient(IElasticClient client)
        {
            _client = client;
        }

        public static async Task<bool> IsActiveAsync()
        {
            var response = await _client.PingAsync();
            return response?.IsValid ?? false;
        }

        public static PluginVersionInfo GetVersion()
        {
            var pluginVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "N/A";
            var umbracoVersion = "umbracoConfigurationStatus".FromAppSettings("N/A");
            return new PluginVersionInfo(pluginVersion, umbracoVersion);
        }

        //public static string ActiveIndexName => _client.ConnectionSettings.DefaultIndex;
        //public static bool HasActiveIndex { get; set; } = false;
    }
}
