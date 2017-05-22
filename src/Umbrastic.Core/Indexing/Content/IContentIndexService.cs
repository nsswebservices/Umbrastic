using Umbraco.Core.Models;

namespace Umbrastic.Core.Indexing.Content
{
    public interface IContentIndexService<in TContent> : IIndexService<TContent> where TContent : IContent
    {
    }
}
