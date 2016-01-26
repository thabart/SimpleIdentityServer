namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Translation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.translations",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 255),
                        LanguageTag = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.Code, t.LanguageTag });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.translations");
        }
    }
}
