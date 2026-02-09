namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSubjectModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subjects", "ScheduleDay", c => c.Int());
            AddColumn("dbo.Subjects", "ScheduleTimeSlot", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subjects", "ScheduleTimeSlot");
            DropColumn("dbo.Subjects", "ScheduleDay");
        }
    }
}
