namespace SimpleIdentityServer.Host 
{
    public sealed class SwaggerOptions
    {
        /// <summary>
        /// Enable or disable swagger.
        /// </summary>
        public bool IsSwaggerEnabled { get; set; }

        /// <summary>
        /// If swagger is enabled and the SimpleIdentityServer is hosted under a relative path, then the URL needs to be specified.
        /// Otherwise the information cannot be extracted from the endpoints.
        /// </summary>
        public string SwaggerUrl { get; set; }
    }   
}