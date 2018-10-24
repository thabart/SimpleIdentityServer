namespace SimpleIdentityServer.Uma.Core
{
    public class UmaConfigurationOptions
    {
        public UmaConfigurationOptions()
        {
            RptLifeTime = 3000;
            TicketLifeTime = 3000;
        }

        /// <summary>
        /// Gets or sets the RPT lifetime (seconds).
        /// </summary>
        public int RptLifeTime { get; set; }
        /// <summary>
        /// Gets or sets the ticket lifetime (seconds).
        /// </summary>
        public int TicketLifeTime { get; set; }
    }
}
