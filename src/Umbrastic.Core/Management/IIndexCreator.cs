using System.Threading.Tasks;

namespace Umbrastic.Core.Management
{
    public interface IIndexCreator
    {
        IIndexCreationResult Create();
        Task<IIndexCreationResult> CreateAsync();
    }
}
