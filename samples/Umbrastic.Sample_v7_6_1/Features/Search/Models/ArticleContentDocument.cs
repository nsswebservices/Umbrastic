using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbrastic.Sample_v7_6_1.Features.Search.Models
{
    [ElasticsearchType(Name = "article")]
    public class ArticleContentDocument : UmbracoDocument
    {
    }
}