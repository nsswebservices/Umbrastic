using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbrastic.Core.Utils;
using Umbrastic.Sample_v7_6_1.Features.Search.Models;

namespace Umbrastic.Sample_v7_6_1.Controllers
{
    [RoutePrefix("search")]
    public class SearchController : Controller
    {
        [Route]
        // GET: Search
        public ActionResult Index(string query)
        {
            var client = UmbracoSearchFactory.Client;

            //var results = client.Search<ArticleContentDocument>(s =>
            //    s.Query(q => q.MultiMatch(m => m
            //        .Fields(f => f.Field(d => d.Title, 1.3)
            //                     .Field(d => d.Summary)
            //                     .Field(d => d.Url)
            //        )
            //    .Query(query))));

            var results = client.Search<UmbracoDocument>(s =>
                s.Type(typeof(ArticleContentDocument))
                .Query(q => q.MultiMatch(m => m
                    .Fields(f => f.Field(d => d.Summary)
                    ).Query(query))));

            //var rawQuery = System.Text.Encoding.UTF8.GetString(results RequestInformation.Request);

            return View(results.Documents);
        }
    }
}