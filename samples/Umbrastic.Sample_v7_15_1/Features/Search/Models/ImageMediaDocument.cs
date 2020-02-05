using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbrastic.Core.Attributes;

namespace Umbrastic.Sample_v7_15_1.Features.Search.Models
{
    [DocumentType(Name = "image")]
    public class ImageMediaDocument : UmbracoDocument
    {
        public string Extension { get; set; }
        public long Size { get; set; }
    }
}