using Nest;
using System;

namespace Umbrastic.Core.Management.Impl
{
    public class IndexCreationResult : IIndexCreationResult
    {
        public IndexCreationResult(bool success, ICreateIndexResponse indexResponse, Exception exception, string indexName)
        {            
            Success = success;
            IndexResponse = indexResponse;
            Exception = exception;
            IndexName = indexName;
        }
        
        public bool Success { get; }
        public ICreateIndexResponse IndexResponse { get; }
        public Exception Exception { get; }

        public string IndexName { get; }
    }
}
