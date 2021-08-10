using System.ComponentModel.DataAnnotations;

namespace SingleExperience.Entities
{
    class StatusBought
    {
        [Key]
        public int StatusId { get; set; }
        public string Description { get; set; }
    }
}
