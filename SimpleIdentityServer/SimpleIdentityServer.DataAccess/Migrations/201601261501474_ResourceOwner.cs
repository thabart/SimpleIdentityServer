namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResourceOwner : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.addresses", "ResourceOwner_Id", "dbo.resourceOwners");
            DropIndex("dbo.addresses", new[] { "ResourceOwner_Id" });
            DropTable("dbo.resourceOwners");
            DropTable("dbo.addresses");
        }
    }
}
