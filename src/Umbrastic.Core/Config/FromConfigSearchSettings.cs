using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbrastic.Core.Utils;

namespace Umbrastic.Core.Config
{
    public class FromConfigSearchSettings : ISearchSettings
    {
        private static readonly string Prefix = $"{UmbrasticConstants.Configuration.Prefix}:";

        public string Host { get; } = nameof(Host).FromAppSettingsWithPrefix(Prefix, "http://localhost:9200");

        public string IndexEnvironmentPrefix { get; } = GetEnvironmentPrefix();

        private static string GetEnvironmentPrefix()
        {
            var value = nameof(IndexEnvironmentPrefix).FromAppSettingsWithPrefix(Prefix, "%COMPUTERNAME%");
            return Environment.ExpandEnvironmentVariables(value).ToLowerInvariant();
        }

        public string IndexName { get; } = nameof(IndexName).FromAppSettingsWithPrefix(Prefix, "umbrastic");

        public IEnumerable<KeyValuePair<string, string>> AdditionalData { get; } = GetAdditionalData($"{Prefix}{nameof(AdditionalData)}:");

        private static IEnumerable<KeyValuePair<string, string>> GetAdditionalData(string prefix)
        {
            var keys = ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            return keys.Select(appKey =>
            {
                var key = appKey.Replace(prefix, "");
                return new KeyValuePair<string, string>(key, ConfigurationManager.AppSettings.Get(appKey));
            });
        }
    }
}