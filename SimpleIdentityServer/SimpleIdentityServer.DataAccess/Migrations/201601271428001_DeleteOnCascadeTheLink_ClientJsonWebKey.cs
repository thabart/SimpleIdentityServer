namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteOnCascadeTheLink_ClientJsonWebKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients");
            AddForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients", "ClientId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients");
            AddForeignKey("dbo.jsonWebKeys", "Client_ClientId", "dbo.clients", "ClientId");
        }
    }
}
