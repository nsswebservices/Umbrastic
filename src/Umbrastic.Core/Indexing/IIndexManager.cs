using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbrastic.Core.Indexing.Impl;

namespace Umbrastic.Core.Indexing
{
    public interface IIndexManager
    {
        Task CreateAsync();
        Task ActivateIndexAsync(string indexName);
        Task DeleteIndexAsync(string indexName);

        Task<IEnumerable<IndexStatusInfo>> IndicesInfo();

        Task<Version> GetElasticsearchVersion();

        Task<JObject> GetIndexMappingInfo(string indexName);
    }
}
