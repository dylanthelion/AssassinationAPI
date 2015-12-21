using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class PlayerGameArchive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerID { get; private set; }
        public int GameID { get; private set; }
        public string TeamName { get; set; }

        public bool Alive { get; set; }

        [ForeignKey("PlayerID")]
        public virtual AccountArchive Player { get; set; }
        [ForeignKey("GameID")]
        public virtual GameArchive Game { get; set; }

        public PlayerGameArchive(AccountArchive aa, GameArchive ga, PlayerGame pg)
        {
            Player = aa;
            Game = ga;
            if (pg.TeamName != null)
            {
                TeamName = pg.TeamName;
            }
            Alive = pg.Alive;
        }
    }
}