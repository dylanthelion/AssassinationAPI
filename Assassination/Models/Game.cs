using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int LocationID { get; set; }
        [ForeignKey("LocationID")]
        public virtual Geocoordinate Location { get; set; }
        [Required]
        public string LocationDescription { get; set; }
        public int NumberOfPlayers { get; set; }
        [DefaultValue(1500.0)]
        public float RadiusInMeters { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime StartTime { get; set; }
        [DefaultValue(false)]
        public bool IsActiveGame { get; set; }
        [DefaultValue(45)]
        public int GameLengthInMinutes { get; set; }

        public Game()
        {
            IsActiveGame = false;
        }

        public Game(string location) : this()
        {
            LocationDescription = location;
        }
    }
}