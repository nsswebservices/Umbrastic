using Nest;
using System.Threading.Tasks;
using Umbrastic.Core.Domain;

namespace Umbrastic.Core.Management
{
    public interface IIndexCreator
    {
        IIndexCreationResult Create(TypeMappingDescriptor<IUmbracoDocument> typeMappingDescriptor);
        Task<IIndexCreationResult> CreateAsync(TypeMappingDescriptor<IUmbracoDocument> typeMappingDescriptor);
    }
}
