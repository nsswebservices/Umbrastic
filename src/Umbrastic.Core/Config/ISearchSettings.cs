using System.Collections.Generic;

namespace Umbrastic.Core.Config
{
    public interface ISearchSettings
    {
        string Host { get; }
        string IndexEnvironmentPrefix { get; }
        string IndexName { get; }

        IEnumerable<KeyValuePair<string, string>> AdditionalData { get; }
    }
}