namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinalModelSetup : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.TaskItems", name: "StudyPlan_Id", newName: "StudyPlanId");
            RenameIndex(table: "dbo.TaskItems", name: "IX_StudyPlan_Id", newName: "IX_StudyPlanId");
            AddColumn("dbo.Subjects", "IsCompleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Subjects", "Credits", c => c.Int(nullable: false));
            AddColumn("dbo.TaskItems", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.TaskItems", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.TaskItems", "PlanedHours", c => c.Int(nullable: false));
            AlterColumn("dbo.Subjects", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.TaskItems", "Title", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Groups", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.StudyPlans", "Name", c => c.String(nullable: false));
            DropColumn("dbo.TaskItems", "IsCompleted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskItems", "IsCompleted", c => c.Boolean(nullable: false));
            AlterColumn("dbo.StudyPlans", "Name", c => c.String());
            AlterColumn("dbo.Groups", "Name", c => c.String());
            AlterColumn("dbo.TaskItems", "Title", c => c.String());
            AlterColumn("dbo.Subjects", "Name", c => c.String());
            DropColumn("dbo.TaskItems", "PlanedHours");
            DropColumn("dbo.TaskItems", "Status");
            DropColumn("dbo.TaskItems", "Priority");
            DropColumn("dbo.Subjects", "Credits");
            DropColumn("dbo.Subjects", "IsCompleted");
            RenameIndex(table: "dbo.TaskItems", name: "IX_StudyPlanId", newName: "IX_StudyPlan_Id");
            RenameColumn(table: "dbo.TaskItems", name: "StudyPlanId", newName: "StudyPlan_Id");
        }
    }
}
