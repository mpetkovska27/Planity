namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskRepeat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskItems", "Repeat", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TaskItems", "Repeat");
        }
    }
}
