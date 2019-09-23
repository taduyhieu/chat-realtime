namespace Chat.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserRoom : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRooms",
                c => new
                    {
                        UserRoomId = c.String(nullable: false, maxLength: 128),
                        RoomId = c.Int(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.UserRoomId)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.RoomId)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRooms", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserRooms", "RoomId", "dbo.Rooms");
            DropIndex("dbo.UserRooms", new[] { "User_Id" });
            DropIndex("dbo.UserRooms", new[] { "RoomId" });
            DropTable("dbo.UserRooms");
        }
    }
}
