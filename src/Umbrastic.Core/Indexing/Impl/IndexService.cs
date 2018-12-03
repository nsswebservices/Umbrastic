using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbrastic.Core.Attributes;
using Umbrastic.Core.Config;
using Umbrastic.Core.Domain;
using Umbrastic.Core.Utils;

namespace Umbrastic.Core.Indexing.Impl
{
    public abstract class IndexService<TUmbracoDocument, TUmbracoEntity, TSearchSettings> : IIndexService<TUmbracoEntity>
          where TUmbracoEntity : class, IContentBase
          where TUmbracoDocument : class, IUmbracoDocument, new()
          where TSearchSettings : ISearchSettings
    {
        private readonly IElasticClient _client;
        private readonly UmbracoContext _umbracoContext;    

        // NH/RH - TODO - refactor, property is no longer required - DocumentTypeName can be used instead
        protected string IndexTypeName => DocumentTypeName;

        protected TSearchSettings SearchSettings { get; }

        protected IndexService(IElasticClient client, UmbracoContext umbracoContext, TSearchSettings searchSettings)
        {
            _client = client;
            _umbracoContext = umbracoContext;
            SearchSettings = searchSettings;         
        }

        protected IndexService(TSearchSettings searchSettings)
            : this(UmbracoSearchFactory.Client, UmbracoContext.Current, searchSettings)
        {
        }

        public void Index(TUmbracoEntity entity, string indexName = null)
        {
            if (!IsExcludedFromIndex(entity))
            {
                var doc = CreateCore(entity);
                IndexCore(_client, doc, indexName);
            }
            else
            {
                Remove(entity, indexName);
            }
        }

        protected virtual void IndexCore(IElasticClient client, TUmbracoDocument document,
            string indexName = null)
        {
            client.Index(document, i => i.Index(indexName).Id(document.Id));
        }

        public TypeMappingDescriptor<IUmbracoDocument> UpdateTypeMappingDescriptor(
            TypeMappingDescriptor<IUmbracoDocument> typeMappingDescriptor)
        {
            if(typeMappingDescriptor == null)
            {
                var ex = new ArgumentNullException("typeMappingDescriptor");
                LogHelper.Error(GetType(), $"Error creating type mapping for entity {typeof(TUmbracoDocument).Name}", ex);
                throw ex;
            }

            return typeMappingDescriptor.AutoMap<TUmbracoDocument>();
        }

        public string EntityTypeName { get; } = typeof(TUmbracoDocument).Name;

        public string DocumentTypeName { get; } =
            typeof(TUmbracoDocument).GetCustomAttribute<DocumentTypeAttribute>()?.Name ?? typeof(TUmbracoDocument).Name;

        public long CountOfDocumentsForIndex(string indexName)
        {
            // NH - TO DO - abstract this query into helper or query base class
            var response = _client.Count<TUmbracoDocument>(c => c
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Term(t => t.Type, DocumentTypeName)
                ))));


            if (response.IsValid)
            {
                return response.Count;
            }
            return -1;
        }

        protected abstract IEnumerable<TUmbracoEntity> RetrieveIndexItems(ServiceContext serviceContext);

        protected virtual void RemoveFromIndex(IList<string> ids, string indexName)
        {
            if (ids.Any())
            {
                UmbracoSearchFactory.Client.Bulk(
                    b => b.DeleteMany<TUmbracoDocument>(ids, (desc, id) => desc.Index(indexName)).Refresh(Refresh.True));
            }
        }

        protected virtual void AddOrUpdateIndex(IList<TUmbracoDocument> docs, string indexName, int pageSize = 500)
        {
            if (docs.Any())
            {
                LogHelper.Info(GetType(), () => $"Indexing {docs.Count} {DocumentTypeName} documents into {indexName}");
                var response = _client.Bulk(b => b.IndexMany(docs, (desc, doc) => desc.Index(indexName).Id(doc.Id)));
                if (response.Errors)
                {
                    LogHelper.Warn(GetType(), $"There were errors during bulk indexing, {response.ItemsWithErrors.Count()} items failed");
                }
                LogHelper.Info(GetType(), () => $"Finished indexing {docs.Count} {DocumentTypeName} documents into {indexName}");
            }

        }

        public void Build(string indexName, Func<ServiceContext, IEnumerable<TUmbracoEntity>> customRetrieveFunc = null)
        {
            var pageSize = IndexBatchSize;
            LogHelper.Info(GetType(), () => $"Starting to index [{DocumentTypeName}] into {indexName} (custom retrieval: {customRetrieveFunc != null})");
            var retrievedItems = customRetrieveFunc?.Invoke(_umbracoContext.Application.Services) ?? RetrieveIndexItems(_umbracoContext.Application.Services);

            foreach (var contentList in retrievedItems.Page(pageSize))
            {
                var contentGroups = contentList.ToLookup(IsExcludedFromIndex, c => c);
                RemoveFromIndex(contentGroups[true].Select(x => x.Id.ToString()).ToList(), indexName);
                AddOrUpdateIndex(contentGroups[false].Select(CreateCore).Where(x => x != null).ToList(), indexName, pageSize);
            }
            _client.Refresh(indexName);

            LogHelper.Info(GetType(), () => $"Finished indexing [{DocumentTypeName}] into {indexName}");
        }

        protected virtual int IndexBatchSize => SearchSettings.GetAdditionalData(UmbrasticConstants.Configuration.IndexBatchSize, 500);

        protected abstract void Create(TUmbracoDocument doc, TUmbracoEntity entity);

        protected virtual void UpdateIndexTypeMappingCore(IElasticClient client, string indexName)
        {
            client.Map<TUmbracoDocument>(m => m.AutoMap().Index(indexName));
        }

        private TUmbracoDocument CreateCore(TUmbracoEntity contentInstance)
        {
            try
            {
                var doc = new TUmbracoDocument
                {
                    Id = IdFor(contentInstance),
                    Url = UrlFor(contentInstance),
                    Type = DocumentTypeName
                };

                Create(doc, contentInstance);

                return doc;
            }
            catch (Exception ex)
            {
                LogHelper.Error(GetType(), $"Unable to create {DocumentTypeName} due to an exception", ex);
                return null;
            }
        }

        public void Remove(TUmbracoEntity entity, string indexName)
        {
            RemoveCore(_client, entity, indexName);
        }

        protected virtual void RemoveCore(IElasticClient client, TUmbracoEntity entity,
            string indexName = null)
        {
            var idValue = IdFor(entity);
            var documentPath = DocumentPath<TUmbracoDocument>.Id(idValue);

            if (client.DocumentExists(documentPath, d => d.Index(indexName)).Exists)
            {
                client.Delete(documentPath, d => d.Index(indexName));
            }
        }

        public virtual bool IsExcludedFromIndex(TUmbracoEntity entity)
        {
            var propertyAlias = SearchSettings.GetAdditionalData(UmbrasticConstants.Configuration.ExcludeFromIndexPropertyAlias, UmbrasticConstants.Properties.ExcludeFromIndexAlias);
            return entity.HasProperty(propertyAlias) && entity.GetValue<bool>(propertyAlias);
        }

        public abstract bool ShouldIndex(TUmbracoEntity entity);

        protected virtual string UrlFor(TUmbracoEntity contentInstance)
        {
            return UmbracoContext.Current.UrlProvider.GetUrl(contentInstance.Id);
        }

        protected virtual string IdFor(TUmbracoEntity contentInstance)
        {
            return contentInstance.Id.ToString();
        }
    }
}
