using Nest;
using System;

namespace Umbrastic.Core.Management
{
    public interface IIndexCreationResult
    {
       
        bool Success { get; }
        ICreateIndexResponse IndexResponse { get; }
        Exception Exception { get; }

        string IndexName { get; }
    }
}
