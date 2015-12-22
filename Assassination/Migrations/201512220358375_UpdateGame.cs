namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateGame : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "MaxGameLengthInMinutes", c => c.Int(nullable: false));
            AddColumn("dbo.GameArchives", "EndTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.Games", "GameLengthInMinutes", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "GameLengthInMinutes");
            DropColumn("dbo.GameArchives", "EndTime");
            DropColumn("dbo.Accounts", "MaxGameLengthInMinutes");
        }
    }
}
