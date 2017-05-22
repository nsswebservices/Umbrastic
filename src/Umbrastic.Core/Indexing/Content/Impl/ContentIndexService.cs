﻿using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbrastic.Core.Config;
using Umbrastic.Core.Domain;
using Umbrastic.Core.Utils;

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
            return serviceContext.ContentService.GetContentOfContentType(contentType.Id).Where(x => x.Published);
        }

        public override sealed bool ShouldIndex(IContent entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
