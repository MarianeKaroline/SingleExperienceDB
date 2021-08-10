using System.ComponentModel.DataAnnotations;

namespace SingleExperience.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Description { get; set; }
    }
}
