using Nest;

namespace Umbrastic.Core.Settings
{
    public interface IIndexSettingCreator
    {
        IIndexState Create();
    }
}
