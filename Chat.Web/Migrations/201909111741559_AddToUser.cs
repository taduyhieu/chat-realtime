namespace Chat.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddToUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Messages", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Messages", new[] { "ApplicationUser_Id" });
            RenameColumn(table: "dbo.Messages", name: "FromUser_Id", newName: "FromUserId");
            RenameColumn(table: "dbo.Messages", name: "ToUser_Id", newName: "ToUserId");
            RenameIndex(table: "dbo.Messages", name: "IX_FromUser_Id", newName: "IX_FromUserId");
            RenameIndex(table: "dbo.Messages", name: "IX_ToUser_Id", newName: "IX_ToUserId");
            DropColumn("dbo.Messages", "ApplicationUser_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "ApplicationUser_Id", c => c.String(maxLength: 128));
            RenameIndex(table: "dbo.Messages", name: "IX_ToUserId", newName: "IX_ToUser_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_FromUserId", newName: "IX_FromUser_Id");
            RenameColumn(table: "dbo.Messages", name: "ToUserId", newName: "ToUser_Id");
            RenameColumn(table: "dbo.Messages", name: "FromUserId", newName: "FromUser_Id");
            CreateIndex("dbo.Messages", "ApplicationUser_Id");
            AddForeignKey("dbo.Messages", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
