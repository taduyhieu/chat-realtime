namespace Chat.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRoleUserRoom : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserRooms", "Role", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserRooms", "Role");
        }
    }
}
