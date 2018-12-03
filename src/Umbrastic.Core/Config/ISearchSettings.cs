using System.Collections.Generic;

namespace Umbrastic.Core.Config
{
    public interface ISearchSettings
    {
        string Host { get; }
        string IndexEnvironmentPrefix { get; }
        string IndexName { get; }
        string DefaultTypeName { get; }

        IEnumerable<KeyValuePair<string, string>> AdditionalData { get; }
    }
}