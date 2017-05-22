using Nest;
using System;
using Umbraco.Core.Logging;
using Umbrastic.Core.Utils;

namespace Umbrastic.Core.Indexing.Content.Impl
{
    public class ContentIndexer : IEntityIndexer
    {
        public void Build(string indexName)
        {
            using (BusyStateManager.Start($"Building content for {indexName}", indexName))
            {
                LogHelper.Info<ContentIndexer>($"Started building index [{indexName}]");
                foreach (var indexService in UmbracoSearchFactory.GetContentIndexServices())
                {
                    try
                    {
                        LogHelper.Info<ContentIndexer>($"Started to index content for {indexService.DocumentTypeName}");
                        BusyStateManager.UpdateMessage($"Indexing {indexService.DocumentTypeName}");

                        
                        indexService.Build(indexName);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<ContentIndexer>($"Failed to index content for {indexService.DocumentTypeName}",
                            ex);
                    }
                }
                LogHelper.Info<ContentIndexer>(
                    $"Finished building index [{indexName}] : elapsed {BusyStateManager.Elapsed.ToString("g")}");
            }
        }
    }
}
