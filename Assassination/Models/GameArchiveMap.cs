using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class GameArchiveMap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int GameID { get; set; }
        [ForeignKey("GameID")]
        public virtual Game Game { get; private set; }
        public int GameArchiveID { get; set; }
        [ForeignKey("GameArchiveID")]
        public virtual GameArchive GameArchive { get; private set; }

        public GameArchiveMap(Game g, GameArchive ga)
        {
            Game = g;
            GameArchive = ga;
        }
    }
}