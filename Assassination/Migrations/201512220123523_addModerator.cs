namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addModerator : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "IsActiveGame", c => c.Boolean(nullable: false));
            AddColumn("dbo.PlayerGames", "IsModerator", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlayerGames", "IsModerator");
            DropColumn("dbo.Games", "IsActiveGame");
        }
    }
}
