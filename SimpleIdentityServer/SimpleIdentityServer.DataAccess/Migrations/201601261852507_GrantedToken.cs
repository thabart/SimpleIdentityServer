namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GrantedToken : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.grantedTokens");
        }
    }
}
