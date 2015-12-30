using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        [Required]
        public string UUID { get; set; }
        public int PlayerID { get; set; }
        [ForeignKey("PlayerID")]
        public virtual Player Owner { get; private set; }

        public Device(Player p, string id) : this()
        {
            Owner = p;
            UUID = id;
        }

        public Device()
        {
        }
    }
}