using System.ComponentModel.DataAnnotations;

namespace SingleExperience.Entities
{
    class StatusProductCart
    {
        [Key]
        public int StatusId { get; set; }
        public string Description { get; set; }
    }
}
