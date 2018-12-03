using Nest;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbrastic.Core.Domain;

namespace Umbrastic.Core.Indexing
{
    /// <summary>
    /// An index service is responsible for the creation and indexing of a node derived from <see cref="IContentBase"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IIndexService<in TEntity> where TEntity : IContentBase
    {
        void Build(string indexName, Func<ServiceContext, IEnumerable<TEntity>> customRetrieveFunc = null);

        void Index(TEntity entity, string indexName);
        void Remove(TEntity entity, string indexName);

        bool IsExcludedFromIndex(TEntity entity);

        bool ShouldIndex(TEntity entity);

        TypeMappingDescriptor<IUmbracoDocument> UpdateTypeMappingDescriptor(
            TypeMappingDescriptor<IUmbracoDocument> typeMappingDescriptor);

        string EntityTypeName { get; }

        string DocumentTypeName { get; }

        long CountOfDocumentsForIndex(string indexName);
    }
}
