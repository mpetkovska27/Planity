namespace Planity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedGroups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "Description", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "Description");
        }
    }
}
