namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskItems", "AttachedFilePath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TaskItems", "AttachedFilePath");
        }
    }
}
