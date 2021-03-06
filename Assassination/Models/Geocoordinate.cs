﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Assassination.Models
{
    public class Geocoordinate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }

        public Geocoordinate()
        {
        }

        public Geocoordinate(float lat, float longi) : this()
        {
            Latitude = lat;
            Longitude = longi;
        }

        public Geocoordinate(float lat, float longi, float alt)
            : this()
        {
            Latitude = lat;
            Longitude = longi;
            Altitude = alt;
        }
    }
}