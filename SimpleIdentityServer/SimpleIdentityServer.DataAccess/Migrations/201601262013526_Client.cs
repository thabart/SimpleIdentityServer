namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Client : DbMigration
    {
        public override void Up()
        {
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
            
            AddColumn("dbo.jsonWebKeys", "Client_ClientId", c => c.String(maxLength: 128));
            CreateIndex("dbo.jsonWebKeys", "Client_ClientId");
            AddForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients", "ClientId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients");
            DropForeignKey("dbo.clientScopes", "ScopeName", "dbo.scopes");
            DropForeignKey("dbo.clientScopes", "ClientId", "dbo.clients");
            DropIndex("dbo.clientScopes", new[] { "ScopeName" });
            DropIndex("dbo.clientScopes", new[] { "ClientId" });
            DropIndex("dbo.jsonWebKeys", new[] { "Client_ClientId" });
            DropColumn("dbo.jsonWebKeys", "Client_ClientId");
            DropTable("dbo.clientScopes");
            DropTable("dbo.clients");
        }
    }
}
