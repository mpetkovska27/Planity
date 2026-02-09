namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskItemsUpdate1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskItems", "ParentTaskId", c => c.Int());
            AlterColumn("dbo.TaskItems", "DueDate", c => c.DateTime());
            AlterColumn("dbo.TaskItems", "PlanedHours", c => c.Int());
            CreateIndex("dbo.TaskItems", "ParentTaskId");
            AddForeignKey("dbo.TaskItems", "ParentTaskId", "dbo.TaskItems", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskItems", "ParentTaskId", "dbo.TaskItems");
            DropIndex("dbo.TaskItems", new[] { "ParentTaskId" });
            AlterColumn("dbo.TaskItems", "PlanedHours", c => c.Int(nullable: false));
            AlterColumn("dbo.TaskItems", "DueDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.TaskItems", "ParentTaskId");
        }
    }
}
