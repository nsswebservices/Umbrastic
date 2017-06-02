using Nest;
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
using Umbrastic.Core.Indexing.Impl;
using Umbrastic.Core.Indexing.Media;

namespace Umbrastic.Core.Indexing.Media.Impl
{
    public abstract class MediaIndexService<TMediaDocument> : MediaIndexService<TMediaDocument, FromConfigSearchSettings>
        where TMediaDocument : class, IUmbracoDocument, new()
    {
        protected MediaIndexService(IElasticClient client, UmbracoContext umbracoContext, FromConfigSearchSettings searchSettings) : base(client, umbracoContext, searchSettings)
        {
        }

        protected MediaIndexService(IElasticClient client, UmbracoContext umbracoContext) : this(client, umbracoContext, SearchSettings<FromConfigSearchSettings>.Current)
        {
        }
    }

    public abstract class MediaIndexService<TMediaDocument, TSearchSettings> : IndexService<TMediaDocument, IMedia, TSearchSettings>, IMediaIndexService<IMedia>
        where TMediaDocument : class, IUmbracoDocument, new()
        where TSearchSettings : ISearchSettings
    {
        protected MediaIndexService(IElasticClient client, UmbracoContext umbracoContext, TSearchSettings searchSettings) : base(client, umbracoContext, searchSettings) { }

        protected override sealed IEnumerable<IMedia> RetrieveIndexItems(ServiceContext serviceContext)
        {
            var mediaType = serviceContext.ContentTypeService.GetMediaType(DocumentTypeName);
            return serviceContext.MediaService.GetMediaOfMediaType(mediaType.Id).Where(x => !x.Trashed);
        }

        public override sealed bool ShouldIndex(IMedia entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
