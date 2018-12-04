using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbrastic.Core.Indexing.Media.Impl;
using Umbrastic.Core.Utils;
using Umbrastic.Sample_v7_12_4.Features.Search.Models;

namespace Umbrastic.Sample_v7_12_4.Features.Search.Services
{
    public class ImageMediaIndexService : MediaIndexService<ImageMediaDocument>
    {
        protected override void Create(ImageMediaDocument doc, IMedia entity)
        {
            doc.Extension = entity.GetValue<string>("umbracoExtension");
            var bytesAttempt = entity.GetValue<string>("umbracoBytes").TryConvertTo<long>();
            if (bytesAttempt.Success)
            {
                doc.Size = bytesAttempt.Result;
            }
        }

        public ImageMediaIndexService() : base(UmbracoSearchFactory.Client, UmbracoContext.Current) { }

    }
}