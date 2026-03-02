namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TaskItems", "AttachedFilePath");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskItems", "AttachedFilePath", c => c.String());
        }
    }
}
