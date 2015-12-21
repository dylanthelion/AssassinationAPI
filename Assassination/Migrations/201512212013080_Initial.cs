namespace Assassination.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountArchives",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        MaxPlayers = c.Int(nullable: false),
                        MaxTeams = c.Int(nullable: false),
                        MaxRadiusInMeters = c.Single(nullable: false),
                        MaxGamesPerWeek = c.Int(nullable: false),
                        Experience = c.Int(nullable: false),
                        MaxKillRadiusInMeters = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Players", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UUID = c.String(nullable: false),
                        PlayerID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Players", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID);
            
            CreateTable(
                "dbo.GameArchives",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Geocoordinates",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Latitude = c.Single(nullable: false),
                        Longitude = c.Single(nullable: false),
                        Altitude = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LocationID = c.Int(nullable: false),
                        LocationDescription = c.String(nullable: false),
                        NumberOfPlayers = c.Int(nullable: false),
                        RadiusInMeters = c.Single(nullable: false),
                        StartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Geocoordinates", t => t.LocationID, cascadeDelete: true)
                .Index(t => t.LocationID);
            
            CreateTable(
                "dbo.PlayerGameArchives",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        GameID = c.Int(nullable: false),
                        TeamName = c.String(),
                        Alive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.GameArchives", t => t.GameID, cascadeDelete: true)
                .ForeignKey("dbo.AccountArchives", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID)
                .Index(t => t.GameID);
            
            CreateTable(
                "dbo.PlayerGames",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        GameID = c.Int(nullable: false),
                        TeamName = c.String(),
                        Latitude = c.Single(nullable: false),
                        Longitude = c.Single(nullable: false),
                        Altitude = c.Single(nullable: false),
                        Alive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Games", t => t.GameID, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID)
                .Index(t => t.GameID);
            
            CreateTable(
                "dbo.TargetArchives",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerGameID = c.Int(nullable: false),
                        TargetID = c.Int(nullable: false),
                        Killed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.PlayerGameArchives", t => t.PlayerGameID, cascadeDelete: true)
                .ForeignKey("dbo.AccountArchives", t => t.TargetID, cascadeDelete: false)
                .Index(t => t.PlayerGameID)
                .Index(t => t.TargetID);
            
            CreateTable(
                "dbo.Targets",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerGameID = c.Int(nullable: false),
                        TargetID = c.Int(nullable: false),
                        Killed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.PlayerGames", t => t.PlayerGameID, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.TargetID, cascadeDelete: false)
                .Index(t => t.PlayerGameID)
                .Index(t => t.TargetID);
            
            CreateTable(
                "dbo.AccountArchiveMaps",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        AccountArchiveID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AccountArchives", t => t.AccountArchiveID, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.PlayerID, cascadeDelete: true)
                .Index(t => t.PlayerID)
                .Index(t => t.AccountArchiveID);
            
            CreateTable(
                "dbo.GameArchiveMaps",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        GameID = c.Int(nullable: false),
                        GameArchiveID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Games", t => t.GameID, cascadeDelete: true)
                .ForeignKey("dbo.GameArchives", t => t.GameArchiveID, cascadeDelete: true)
                .Index(t => t.GameID)
                .Index(t => t.GameArchiveID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameArchiveMaps", "GameArchiveID", "dbo.GameArchives");
            DropForeignKey("dbo.GameArchiveMaps", "GameID", "dbo.Games");
            DropForeignKey("dbo.AccountArchiveMaps", "PlayerID", "dbo.Players");
            DropForeignKey("dbo.AccountArchiveMaps", "AccountArchiveID", "dbo.AccountArchives");
            DropForeignKey("dbo.Targets", "TargetID", "dbo.Players");
            DropForeignKey("dbo.Targets", "PlayerGameID", "dbo.PlayerGames");
            DropForeignKey("dbo.TargetArchives", "TargetID", "dbo.AccountArchives");
            DropForeignKey("dbo.TargetArchives", "PlayerGameID", "dbo.PlayerGameArchives");
            DropForeignKey("dbo.PlayerGames", "PlayerID", "dbo.Players");
            DropForeignKey("dbo.PlayerGames", "GameID", "dbo.Games");
            DropForeignKey("dbo.PlayerGameArchives", "PlayerID", "dbo.AccountArchives");
            DropForeignKey("dbo.PlayerGameArchives", "GameID", "dbo.GameArchives");
            DropForeignKey("dbo.Games", "LocationID", "dbo.Geocoordinates");
            DropForeignKey("dbo.Devices", "PlayerID", "dbo.Players");
            DropForeignKey("dbo.Accounts", "PlayerID", "dbo.Players");
            DropIndex("dbo.GameArchiveMaps", new[] { "GameArchiveID" });
            DropIndex("dbo.GameArchiveMaps", new[] { "GameID" });
            DropIndex("dbo.AccountArchiveMaps", new[] { "AccountArchiveID" });
            DropIndex("dbo.AccountArchiveMaps", new[] { "PlayerID" });
            DropIndex("dbo.Targets", new[] { "TargetID" });
            DropIndex("dbo.Targets", new[] { "PlayerGameID" });
            DropIndex("dbo.TargetArchives", new[] { "TargetID" });
            DropIndex("dbo.TargetArchives", new[] { "PlayerGameID" });
            DropIndex("dbo.PlayerGames", new[] { "GameID" });
            DropIndex("dbo.PlayerGames", new[] { "PlayerID" });
            DropIndex("dbo.PlayerGameArchives", new[] { "GameID" });
            DropIndex("dbo.PlayerGameArchives", new[] { "PlayerID" });
            DropIndex("dbo.Games", new[] { "LocationID" });
            DropIndex("dbo.Devices", new[] { "PlayerID" });
            DropIndex("dbo.Accounts", new[] { "PlayerID" });
            DropTable("dbo.GameArchiveMaps");
            DropTable("dbo.AccountArchiveMaps");
            DropTable("dbo.Targets");
            DropTable("dbo.TargetArchives");
            DropTable("dbo.PlayerGames");
            DropTable("dbo.PlayerGameArchives");
            DropTable("dbo.Games");
            DropTable("dbo.Geocoordinates");
            DropTable("dbo.GameArchives");
            DropTable("dbo.Devices");
            DropTable("dbo.Players");
            DropTable("dbo.Accounts");
            DropTable("dbo.AccountArchives");
        }
    }
}
