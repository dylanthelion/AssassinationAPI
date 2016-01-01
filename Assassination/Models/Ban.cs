using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Ban
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerID { get; private set; }
        public int GameID { get; private set; }

        [ForeignKey("PlayerID")]
        public virtual Player Player { get; set; }
        [ForeignKey("GameID")]
        public virtual Game Game { get; set; }

        public Ban()
        {
        }

        public Ban(Player p, Game g) : this()
        {
            Player = p;
            Game = g;
        }
    }
}