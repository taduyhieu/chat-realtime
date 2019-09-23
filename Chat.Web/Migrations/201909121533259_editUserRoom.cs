namespace Chat.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editUserRoom : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.UserRooms", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.UserRooms", name: "IX_User_Id", newName: "IX_UserId");
            DropPrimaryKey("dbo.UserRooms");
            AddColumn("dbo.UserRooms", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.UserRooms", "Id");
            DropColumn("dbo.UserRooms", "UserRoomId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserRooms", "UserRoomId", c => c.String(nullable: false, maxLength: 128));
            DropPrimaryKey("dbo.UserRooms");
            DropColumn("dbo.UserRooms", "Id");
            AddPrimaryKey("dbo.UserRooms", "UserRoomId");
            RenameIndex(table: "dbo.UserRooms", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.UserRooms", name: "UserId", newName: "User_Id");
        }
    }
}
