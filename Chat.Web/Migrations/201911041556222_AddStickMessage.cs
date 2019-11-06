namespace Chat.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStickMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "stick", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "stick");
        }
    }
}
