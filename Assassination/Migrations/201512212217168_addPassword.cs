namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Players", "Password", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Players", "Password");
        }
    }
}
