using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class AccountArchiveMap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int PlayerID { get; set; }
        public int AccountArchiveID { get; set; }
        [ForeignKey("PlayerID")]
        public virtual Player Player { get; private set; }
        [ForeignKey("AccountArchiveID")]
        public virtual AccountArchive AccountArchive { get; private set; }

        public AccountArchiveMap(Player p, AccountArchive aa)
        {
            Player = p;
            AccountArchive = aa;
        }
    }
}