using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbrastic.Core.Config;
using Umbrastic.Core.Domain;
using Umbrastic.Core.Indexing.Impl;

namespace Umbrastic.Core.Indexing.Content.Impl
{
    public abstract class ContentIndexService<TUmbracoDocument> :
        ContentIndexService<TUmbracoDocument, FromConfigSearchSettings>
        where TUmbracoDocument : class, IUmbracoDocument, new()

    {
        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext, FromConfigSearchSettings searchSettings) : base(client, umbracoContext, searchSettings)
        {
        }

        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext) : this(client, umbracoContext, SearchSettings<FromConfigSearchSettings>.Current)
        {
        }
    }


    public abstract class ContentIndexService<TUmbracoDocument, TSearchSettings> : IndexService<TUmbracoDocument, IContent, TSearchSettings>, IContentIndexService<IContent>
        where TUmbracoDocument : class, IUmbracoDocument, new()
        where TSearchSettings : ISearchSettings
    {
        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext, TSearchSettings searchSettings) : base(client, umbracoContext, searchSettings) { }

        protected override sealed IEnumerable<IContent> RetrieveIndexItems(ServiceContext serviceContext)
        {
            var contentType = serviceContext.ContentTypeService.GetContentType(DocumentTypeName);

            // get the published version of each indexable content node of this type
            return serviceContext.ContentService.GetContentOfContentType(contentType.Id)
                .Where(x => x.HasPublishedVersion)
                .Select(x => x.Published ? x : serviceContext.ContentService.GetPublishedVersion(x));
        }

        public override sealed bool ShouldIndex(IContent entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }

        protected override string UrlFor(IContent contentInstance)
        {
            var url = base.UrlFor(contentInstance);

            // on first publish the url will be # so manually build it
            if (url == "#")
            {
                var helper = new UmbracoHelper(UmbracoContext.Current);
                var parent = helper.TypedContent(contentInstance.ParentId);
                url = parent != null ? $"{parent.Url}{contentInstance.Name.ToUrlSegment()}/" : url;
            }

            return url;
        }
    }
}
