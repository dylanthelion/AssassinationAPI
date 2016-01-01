namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bans : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bans",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        GameID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Games", t => t.GameID, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID)
                .Index(t => t.GameID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bans", "PlayerID", "dbo.Players");
            DropForeignKey("dbo.Bans", "GameID", "dbo.Games");
            DropIndex("dbo.Bans", new[] { "GameID" });
            DropIndex("dbo.Bans", new[] { "PlayerID" });
            DropTable("dbo.Bans");
        }
    }
}
