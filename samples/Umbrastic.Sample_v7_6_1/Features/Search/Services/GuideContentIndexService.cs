using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbrastic.Core.Indexing.Content.Impl;
using Umbrastic.Core.Utils;
using Umbrastic.Sample_v7_6_1.Features.Search.Models;

namespace Umbrastic.Sample_v7_6_1.Features.Search.Services
{
    public class GuideContentIndexService : ContentIndexService<GuideContentDocument>
    {
        public GuideContentIndexService() : base(UmbracoSearchFactory.Client, UmbracoContext.Current) { }
        protected override void Create(GuideContentDocument doc, IContent content)
        {
            doc.Title = content.Name;
            doc.Summary = content.GetValue<string>("summary");
        }
    }
}