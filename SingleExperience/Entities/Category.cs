using System.ComponentModel.DataAnnotations;

namespace SingleExperience.Entities
{
    class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Description { get; set; }
    }
}
