using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SingleExperience.Entities
{
    class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public string Description { get; set; }
    }
}
