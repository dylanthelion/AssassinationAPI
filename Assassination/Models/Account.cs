using Assassination.Helpers;
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

        [DefaultValue(Constants.DEFAULTMAXPLAYERS)]
        public int MaxPlayers { get; set; }
        [DefaultValue(Constants.DEFAULTMAXTEAMS)]
        public int MaxTeams { get; set; }
        [DefaultValue(Constants.DEFAULTMAXGAMERADIUS)]
        public float MaxRadiusInMeters { get; set; }
        [DefaultValue(Constants.DEFAULTMAXGAMESPERWEEK)]
        public int MaxGamesPerWeek { get; set; }
        public int Experience { get; set; }
        [DefaultValue(Constants.DEFAULTMAXKILLRADIUS)]
        public float MaxKillRadiusInMeters { get; set; }
        [DefaultValue(Constants.DEFAULTMAXGAMELENGTH)]
        public int MaxGameLengthInMinutes { get; set; }

        public Account(Player p) : this()
        {
            Player = p;
        }

        public Account()
        {
            MaxGameLengthInMinutes = Constants.DEFAULTMAXGAMELENGTH;
            MaxGamesPerWeek = Constants.DEFAULTMAXGAMESPERWEEK;
            MaxKillRadiusInMeters = Constants.DEFAULTMAXKILLRADIUS;
            MaxPlayers = Constants.DEFAULTMAXPLAYERS;
            MaxRadiusInMeters = Constants.DEFAULTMAXGAMERADIUS;
            MaxTeams = Constants.DEFAULTMAXTEAMS;
        }
    }
}