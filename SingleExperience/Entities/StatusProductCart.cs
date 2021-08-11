﻿using SingleExperience.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleExperience.Entities
{
    public class StatusProductCart
    {
        [Key]
        [Column("StatusProductId")]
        public StatusProductEnum StatusProductEnum { get; set; }
        public string Description { get; set; }
    }
}
