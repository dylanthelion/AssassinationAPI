using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class PlayerGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerID { get; private set; }
        public int GameID { get; private set; }
        public string TeamName { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public bool Alive { get; set; }
        public bool IsModerator { get; set; }

        [ForeignKey("PlayerID")]
        public virtual Player Player { get; set; }
        [ForeignKey("GameID")]
        public virtual Game Game { get; set; }

        public PlayerGame(Player p, Game g) : this()
        {
            Player = p;
            Game = g;
        }

        public PlayerGame()
        {
        }
    }
}