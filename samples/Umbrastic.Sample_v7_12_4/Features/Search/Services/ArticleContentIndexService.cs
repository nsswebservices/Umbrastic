using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbrastic.Core.Indexing.Content.Impl;
using Umbrastic.Core.Utils;
using Umbrastic.Sample_v7_12_4.Features.Search.Models;

namespace Umbrastic.Sample_v7_12_4.Features.Search.Services
{
    public class ArticleContentIndexService : ContentIndexService<ArticleContentDocument>
    {
        public ArticleContentIndexService() : base(UmbracoSearchFactory.Client, UmbracoContext.Current) { }
        protected override void Create(ArticleContentDocument doc, IContent content)
        {
            doc.Title = content.Name;
            doc.Summary = content.GetValue<string>("summary");
        }
    }
}