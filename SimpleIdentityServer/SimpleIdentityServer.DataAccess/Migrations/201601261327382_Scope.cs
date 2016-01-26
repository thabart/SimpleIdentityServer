namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Scope : DbMigration
    {
        public override void Up()
        {
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
                "dbo.claims",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Code);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.scopeClaims", "ClaimCode", "dbo.claims");
            DropForeignKey("dbo.scopeClaims", "ScopeName", "dbo.scopes");
            DropIndex("dbo.scopeClaims", new[] { "ClaimCode" });
            DropIndex("dbo.scopeClaims", new[] { "ScopeName" });
            DropTable("dbo.scopeClaims");
            DropTable("dbo.claims");
            DropTable("dbo.scopes");
        }
    }
}
