namespace Umbrastic.Core.Config
{
    public static class UmbrasticConstants
    {
        public static class Configuration
        {
            public const string Prefix = "umbrastic";
            public const string IndexBatchSize = nameof(IndexBatchSize);
            public const string ExcludeFromIndexPropertyAlias = nameof(ExcludeFromIndexPropertyAlias);
            //public const string DisableContentCacheUpdatedEventHook = nameof(DisableContentCacheUpdatedEventHook);
        }

        public static class Properties
        {
            public const string ExcludeFromIndexAlias = "umbrasticExcludeFromIndex";
        }
    }
}
