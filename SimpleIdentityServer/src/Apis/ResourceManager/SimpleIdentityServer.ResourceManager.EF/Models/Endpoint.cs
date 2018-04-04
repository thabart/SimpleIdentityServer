﻿using System;

namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class Endpoint
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ManagerUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public int Type { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}