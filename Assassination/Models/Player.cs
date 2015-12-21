using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }

    public class AssassinationContext : DbContext
    {
        public AssassinationContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<Account> AllAccounts { get; set; }
        public DbSet<AccountArchive> AllAccountArchives { get; set; }
        public DbSet<AccountArchiveMap> AppAccountArchiveMap { get; set; }
        public DbSet<Device> AllDevices { get; set; }
        public DbSet<Game> AllGames { get; set; }
        public DbSet<GameArchive> AllGameArchives { get; set; }
        public DbSet<GameArchiveMap> AppGameArchiveMap { get; set; }
        public DbSet<Geocoordinate> AllGameCoords { get; set; }
        public DbSet<Player> AllPlayers { get; set; }
        public DbSet<PlayerGame> AllPlayerGames { get; set; }
        public DbSet<PlayerGameArchive> AllPlayerGameArchives { get; set; }
        public DbSet<Target> AllTargets { get; set; }
        public DbSet<TargetArchive> AllTargetArchives { get; set; }
    }
}