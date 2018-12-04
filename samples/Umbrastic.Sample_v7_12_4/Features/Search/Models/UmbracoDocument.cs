using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbrastic.Core.Domain;

namespace Umbrastic.Sample_v7_12_4.Features.Search.Models
{
    public class UmbracoDocument : IUmbracoDocument
    {
        [Keyword]
        public string Id { get; set; }

        [Text(Analyzer = "indexy_english")]
        public string Title { get; set; }

        [Text(Analyzer = "indexy_english")]
        public string Summary { get; set; }

        [Keyword]
        public string Url { get; set; }

        [Keyword]
        public string Type { get; set; }
    }
}