using Umbraco.Core.Models;

namespace Umbrastic.Core.Indexing.Media
{
    public interface IMediaIndexService<in TMedia> : IIndexService<TMedia> where TMedia : IMedia
    {
    }
}
