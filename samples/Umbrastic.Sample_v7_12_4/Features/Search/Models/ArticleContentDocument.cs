using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbrastic.Core.Attributes;

namespace Umbrastic.Sample_v7_12_4.Features.Search.Models
{
    [DocumentType(Name = "article")]
    public class ArticleContentDocument : UmbracoDocument
    {
    }
}