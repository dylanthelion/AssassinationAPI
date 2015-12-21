using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Target
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerGameID { get; private set; }
        [ForeignKey("PlayerGameID")]
        public virtual PlayerGame PlayerGame { get; set; }
        public int TargetID { get; set; }
        [ForeignKey("TargetID")]
        public virtual Player TargetPlayer { get; set; }
        public bool Killed { get; set; }

        public Target(PlayerGame pg, Player p)
        {
            PlayerGame = pg;
            TargetPlayer = p;
        }
    }
}