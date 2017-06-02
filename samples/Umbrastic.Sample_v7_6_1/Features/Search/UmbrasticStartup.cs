using Nest;
using Nest.Indexing.Management;
using Nest.Indexing.Management.Impl;
using Nest.Indexing.Settings;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbrastic.Core.EventHandlers;
using Umbrastic.Core.Indexing.Content;
using Umbrastic.Core.Indexing.Media;
using Umbrastic.Sample_v7_6_1.Features.Search.Services;

namespace Umbrastic.Sample_v7_6_1.Features.Search
{
    public class UmbrasticStartup : SearchApplicationEventHandler
    {
        public UmbrasticStartup()
        {
        }

        protected override IIndexCreator GetIndexCreationStrategy(IElasticClient client)
        {
            return new UmbrasticIndexCreationStrategy(client, new IndexSettingCreator());
        }

        protected override IEnumerable<IContentIndexService<IContent>> RegisterContentIndexingServices()
        {
            yield return new ArticleContentIndexService();
        }

        protected override IEnumerable<IMediaIndexService<IMedia>> RegisterMediaIndexingServices()
        {
            yield return new ImageMediaIndexService();
        }

        internal class UmbrasticIndexCreationStrategy : IndexCreator
        {
            public UmbrasticIndexCreationStrategy(IElasticClient client, IIndexSettingCreator settings) : base(client, settings)
            {
            }
        }
    }
}