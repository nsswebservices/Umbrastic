using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;
using Umbrastic.Core.Utils;
using Umbrastic.Core.Management;
using Umbrastic.Core.Domain;

namespace Umbrastic.Core.Indexing.Impl
{
    public enum IndexStatusOption
    {
        None,
        Active,
        Busy
    }

    public struct IndexStatusInfo
    {
        public string Name { get; set; }
        public long DocCount { get; set; }
        public long Queries { get; set; }
        public double SizeInBytes { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public IndexStatusOption Status { get; set; }
    }

    public class IndexManager : IIndexManager
    {
        private readonly IElasticClient _client;
        private readonly IIndexCreator _indexStrategy;

        public IndexManager() : this(UmbracoSearchFactory.Client, UmbracoSearchFactory.GetIndexStrategy()) { }

        public IndexManager(IElasticClient client, IIndexCreator indexStrategy)
        {
            _client = client;
            _indexStrategy = indexStrategy;
        }

        public async Task CreateAsync()
        {
            var typeMappingDescriptor = new TypeMappingDescriptor<IUmbracoDocument>();

            foreach(var c in UmbracoSearchFactory.GetContentIndexServices())
            {
                typeMappingDescriptor = c.UpdateTypeMappingDescriptor(typeMappingDescriptor);
            }

            foreach (var m in UmbracoSearchFactory.GetMediaIndexServices())
            {
                typeMappingDescriptor = m.UpdateTypeMappingDescriptor(typeMappingDescriptor);
            }

            await _indexStrategy.CreateAsync(typeMappingDescriptor);
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            using (
                BusyStateManager.Start(
                    $"Deleting {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                await _client.DeleteIndexAsync(new DeleteIndexRequest(indexName));
            }
        }

        public async Task<IEnumerable<IndexStatusInfo>> IndicesInfo()
        {
            var response = await _client.IndicesStatsAsync(Indices.All);
            var indexAliasName = _client.ConnectionSettings.DefaultIndex;
            return response.Indices.Where(x => x.Key.StartsWith($"{indexAliasName}-")).Select(x => new IndexStatusInfo
            {
                Name = x.Key,
                DocCount = x.Value.Total.Documents.Count,
                Queries = x.Value.Total.Search.QueryTotal,
                SizeInBytes = x.Value.Total.Store.SizeInBytes,
                Status = GetStatus(x.Key)
            });
        }

        public async Task<Version> GetElasticsearchVersion()
        {
            var info = await _client.RootNodeInfoAsync();
            return Version.Parse(info.IsValid ? info.Version.Number : "0.0.0");
        }

        public async Task<JObject> GetIndexMappingInfo(string indexName)
        {
            var response = await _client.GetMappingAsync(new GetMappingRequest(indexName, "*"));

            // TODO : Validate here
            var mappings = response.IsValid ? response.Indices : new ReadOnlyDictionary<IndexName, IndexMappings>(null);
            var stream = new MemoryStream();
            _client.SourceSerializer.Serialize(mappings, stream);
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            string jsonContent;
            using (var r = new StreamReader(stream))
            {
                jsonContent = r.ReadToEnd();
            }

            return string.IsNullOrWhiteSpace(jsonContent) ? null : JObject.Parse(jsonContent);
        }

        private IndexStatusOption GetStatus(string indexName)
        {
            if (BusyStateManager.IsBusy && BusyStateManager.IndexName.Equals(indexName, StringComparison.InvariantCultureIgnoreCase)) return IndexStatusOption.Busy;
            return _client.AliasExists(x => x.Index(indexName).Name(_client.ConnectionSettings.DefaultIndex)).Exists ? IndexStatusOption.Active : IndexStatusOption.None;
        }

        public async Task ActivateIndexAsync(string indexName)
        {
            using (BusyStateManager.Start($"Activating {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                var client = UmbracoSearchFactory.Client;
                var indexAliasName = client.ConnectionSettings.DefaultIndex;
                await client.AliasAsync(a => a
                    .Remove(r => r.Alias(indexAliasName).Index($"{indexAliasName}*"))
                    .Add(aa => aa.Alias(indexAliasName).Index(indexName))
                    );
            }
        }
    }
}
