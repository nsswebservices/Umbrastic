﻿namespace Umbrastic.Core.Domain
{
    /// <summary>
    /// Represents the minimal requirements of the document to be indexed
    /// </summary>
    public interface IUmbracoDocument
    {
        /// <summary>
        /// Gets or sets the Id field for the document, this is typically the id of the umbraco node
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Gets or sets the Url to the node represented by this document
        /// </summary>
        /// <remarks>Recommendation: make this a relative url</remarks>
        string Url { get; set; }
        /// <summary>
        /// Gets or sets the Type of this document, this is typically the umbraco document type alias.
        /// </summary>
        string Type { get; set; }
    }
}
