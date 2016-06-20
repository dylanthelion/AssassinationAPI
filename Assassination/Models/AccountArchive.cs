using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class AccountArchive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        [Required]
        public string UserName { get; set; }

        public AccountArchive(Player p) : this()
        {
            UserName = p.UserName;
        }

        public AccountArchive()
        {

        }
    }
}