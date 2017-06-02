using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbrastic.Sample_v7_6_1.Features.Search.Models
{
    [ElasticsearchType(Name = "image")]
    public class ImageMediaDocument : UmbracoDocument
    {
        public string Extension { get; set; }
        public long Size { get; set; }
    }
}