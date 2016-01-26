namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JsonWebKey : DbMigration
    {
        public override void Up()
        {
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
                    })
                .PrimaryKey(t => t.Kid);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.jsonWebKeys");
        }
    }
}
