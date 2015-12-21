using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class TargetArchive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerGameID { get; private set; }
        [ForeignKey("PlayerGameID")]
        public virtual PlayerGameArchive PlayerGame { get; set; }
        public int TargetID { get; set; }
        [ForeignKey("TargetID")]
        public virtual AccountArchive Target { get; set; }
        public bool Killed { get; set; }

        public TargetArchive(PlayerGameArchive pga, AccountArchive aa, Target t)
        {
            PlayerGame = pga;
            Target = aa;
            Killed = t.Killed;
        }
    }
}