using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbrastic.Core.Attributes
{
    public class DocumentTypeAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
