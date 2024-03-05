﻿using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Driver : BaseClass
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Company { get; set; }

        public byte[]? DriversLicence { get; set; }

        public byte[]? PassportFile { get; set; }
    }
}
