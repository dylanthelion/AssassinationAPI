using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        [Required]
        public int PlayerID { get; set; }
        [ForeignKey("PlayerID")]
        public virtual Player Player { get; set; }

        // Server-specific account stats

        [DefaultValue(5)]
        public int MaxPlayers { get; set; }
        [DefaultValue(2)]
        public int MaxTeams { get; set; }
        [DefaultValue(1500.0)]
        public float MaxRadiusInMeters { get; set; }
        [DefaultValue(3)]
        public int MaxGamesPerWeek { get; set; }
        public int Experience { get; set; }
        [DefaultValue(10.0)]

        public float MaxKillRadiusInMeters { get; set; }

        public Account(Player p)
        {
            Player = p;
        }
    }
}