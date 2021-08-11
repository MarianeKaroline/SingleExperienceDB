using SingleExperience.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleExperience.Entities
{
    public class StatusBought
    {
        [Key]
        [Column("StatusBoughtId")]
        public StatusBoughtEnum StatusBoughtEnum { get; set; }
        public string Description { get; set; }
    }
}
