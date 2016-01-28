namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialize : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Formatted = c.String(),
                        StreetAddress = c.String(),
                        Locality = c.String(),
                        Region = c.String(),
                        PostalCode = c.String(),
                        Country = c.String(),
                        ResourceOwner_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.resourceOwners", t => t.ResourceOwner_Id)
                .Index(t => t.ResourceOwner_Id);
            
            CreateTable(
                "dbo.resourceOwners",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        GivenName = c.String(),
                        FamilyName = c.String(),
                        MiddleName = c.String(),
                        NickName = c.String(),
                        PreferredUserName = c.String(),
                        Profile = c.String(),
                        Picture = c.String(),
                        WebSite = c.String(),
                        Email = c.String(),
                        EmailVerified = c.Boolean(nullable: false),
                        Gender = c.String(),
                        BirthDate = c.String(),
                        ZoneInfo = c.String(),
                        Locale = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberVerified = c.Boolean(nullable: false),
                        UpdatedAt = c.Double(nullable: false),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.consents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Client_ClientId = c.String(nullable: false, maxLength: 128),
                        ResourceOwner_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.clients", t => t.Client_ClientId, cascadeDelete: true)
                .ForeignKey("dbo.resourceOwners", t => t.ResourceOwner_Id, cascadeDelete: true)
                .Index(t => t.Client_ClientId)
                .Index(t => t.ResourceOwner_Id);
            
            CreateTable(
                "dbo.claims",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "dbo.scopes",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Description = c.String(maxLength: 255),
                        IsOpenIdScope = c.Boolean(nullable: false),
                        IsDisplayedInConsent = c.Boolean(nullable: false),
                        IsExposed = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.clients",
                c => new
                    {
                        ClientId = c.String(nullable: false, maxLength: 128),
                        ClientSecret = c.String(),
                        ClientName = c.String(),
                        LogoUri = c.String(),
                        ClientUri = c.String(),
                        PolicyUri = c.String(),
                        TosUri = c.String(),
                        IdTokenSignedResponseAlg = c.String(),
                        IdTokenEncryptedResponseAlg = c.String(),
                        IdTokenEncryptedResponseEnc = c.String(),
                        TokenEndPointAuthMethod = c.Int(nullable: false),
                        ResponseTypes = c.String(),
                        GrantTypes = c.String(),
                        RedirectionUrls = c.String(),
                        ApplicationType = c.Int(nullable: false),
                        JwksUri = c.String(),
                        Contacts = c.String(),
                        SectorIdentifierUri = c.String(),
                        SubjectType = c.String(),
                        UserInfoSignedResponseAlg = c.String(),
                        UserInfoEncryptedResponseAlg = c.String(),
                        UserInfoEncryptedResponseEnc = c.String(),
                        RequestObjectSigningAlg = c.String(),
                        RequestObjectEncryptionAlg = c.String(),
                        RequestObjectEncryptionEnc = c.String(),
                        TokenEndPointAuthSigningAlg = c.String(),
                        DefaultMaxAge = c.Double(nullable: false),
                        RequireAuthTime = c.Boolean(nullable: false),
                        DefaultAcrValues = c.String(),
                        InitiateLoginUri = c.String(),
                        RequestUris = c.String(),
                    })
                .PrimaryKey(t => t.ClientId);
            
            CreateTable(
                "dbo.jsonWebKeys",
                c => new
                    {
                        Kid = c.String(nullable: false, maxLength: 128),
                        Kty = c.Int(nullable: false),
                        Use = c.Int(nullable: false),
                        KeyOps = c.String(),
                        Alg = c.Int(nullable: false),
                        X5u = c.String(),
                        X5t = c.String(),
                        X5tS256 = c.String(),
                        SerializedKey = c.String(),
                        Client_ClientId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Kid)
                .ForeignKey("dbo.clients", t => t.Client_ClientId, cascadeDelete: true)
                .Index(t => t.Client_ClientId);
            
            CreateTable(
                "dbo.authorizationCodes",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        RedirectUri = c.String(),
                        CreateDateTime = c.DateTime(nullable: false),
                        ClientId = c.String(),
                        IdTokenPayload = c.String(),
                        UserInfoPayLoad = c.String(),
                        Scopes = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "dbo.grantedTokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessToken = c.String(),
                        RefreshToken = c.String(),
                        Scope = c.String(),
                        ExpiresIn = c.Int(nullable: false),
                        CreateDateTime = c.DateTime(nullable: false),
                        ClientId = c.String(),
                        UserInfoPayLoad = c.String(),
                        IdTokenPayLoad = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.translations",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 255),
                        LanguageTag = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.Code, t.LanguageTag });
            
            CreateTable(
                "dbo.scopeClaims",
                c => new
                    {
                        ScopeName = c.String(nullable: false, maxLength: 128),
                        ClaimCode = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ScopeName, t.ClaimCode })
                .ForeignKey("dbo.scopes", t => t.ScopeName, cascadeDelete: true)
                .ForeignKey("dbo.claims", t => t.ClaimCode, cascadeDelete: true)
                .Index(t => t.ScopeName)
                .Index(t => t.ClaimCode);
            
            CreateTable(
                "dbo.clientScopes",
                c => new
                    {
                        ClientId = c.String(nullable: false, maxLength: 128),
                        ScopeName = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ClientId, t.ScopeName })
                .ForeignKey("dbo.clients", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("dbo.scopes", t => t.ScopeName, cascadeDelete: true)
                .Index(t => t.ClientId)
                .Index(t => t.ScopeName);
            
            CreateTable(
                "dbo.consentClaims",
                c => new
                    {
                        ConsentId = c.Int(nullable: false),
                        ClaimCode = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ConsentId, t.ClaimCode })
                .ForeignKey("dbo.consents", t => t.ConsentId, cascadeDelete: true)
                .ForeignKey("dbo.claims", t => t.ClaimCode, cascadeDelete: true)
                .Index(t => t.ConsentId)
                .Index(t => t.ClaimCode);
            
            CreateTable(
                "dbo.consentScopes",
                c => new
                    {
                        ConsentId = c.Int(nullable: false),
                        ScopeName = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ConsentId, t.ScopeName })
                .ForeignKey("dbo.consents", t => t.ConsentId, cascadeDelete: true)
                .ForeignKey("dbo.scopes", t => t.ScopeName, cascadeDelete: true)
                .Index(t => t.ConsentId)
                .Index(t => t.ScopeName);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.consents", "ResourceOwner_Id", "dbo.resourceOwners");
            DropForeignKey("dbo.consentScopes", "ScopeName", "dbo.scopes");
            DropForeignKey("dbo.consentScopes", "ConsentId", "dbo.consents");
            DropForeignKey("dbo.consents", "Client_ClientId", "dbo.clients");
            DropForeignKey("dbo.consentClaims", "ClaimCode", "dbo.claims");
            DropForeignKey("dbo.consentClaims", "ConsentId", "dbo.consents");
            DropForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients");
            DropForeignKey("dbo.clientScopes", "ScopeName", "dbo.scopes");
            DropForeignKey("dbo.clientScopes", "ClientId", "dbo.clients");
            DropForeignKey("dbo.scopeClaims", "ClaimCode", "dbo.claims");
            DropForeignKey("dbo.scopeClaims", "ScopeName", "dbo.scopes");
            DropForeignKey("dbo.addresses", "ResourceOwner_Id", "dbo.resourceOwners");
            DropIndex("dbo.consentScopes", new[] { "ScopeName" });
            DropIndex("dbo.consentScopes", new[] { "ConsentId" });
            DropIndex("dbo.consentClaims", new[] { "ClaimCode" });
            DropIndex("dbo.consentClaims", new[] { "ConsentId" });
            DropIndex("dbo.clientScopes", new[] { "ScopeName" });
            DropIndex("dbo.clientScopes", new[] { "ClientId" });
            DropIndex("dbo.scopeClaims", new[] { "ClaimCode" });
            DropIndex("dbo.scopeClaims", new[] { "ScopeName" });
            DropIndex("dbo.jsonWebKeys", new[] { "Client_ClientId" });
            DropIndex("dbo.consents", new[] { "ResourceOwner_Id" });
            DropIndex("dbo.consents", new[] { "Client_ClientId" });
            DropIndex("dbo.addresses", new[] { "ResourceOwner_Id" });
            DropTable("dbo.consentScopes");
            DropTable("dbo.consentClaims");
            DropTable("dbo.clientScopes");
            DropTable("dbo.scopeClaims");
            DropTable("dbo.translations");
            DropTable("dbo.grantedTokens");
            DropTable("dbo.authorizationCodes");
            DropTable("dbo.jsonWebKeys");
            DropTable("dbo.clients");
            DropTable("dbo.scopes");
            DropTable("dbo.claims");
            DropTable("dbo.consents");
            DropTable("dbo.resourceOwners");
            DropTable("dbo.addresses");
        }
    }
}
