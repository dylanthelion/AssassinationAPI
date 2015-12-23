namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GameType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "GameType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "GameType");
        }
    }
}
