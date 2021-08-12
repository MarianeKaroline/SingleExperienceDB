using SingleExperience.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleExperience.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Detail { get; set; }
        public int Amount { get; set; }

        //FK - Category
        [Column("CategoryId")]
        public CategoryEnum CategoryEnum { get; set; }
        [ForeignKey(nameof(CategoryEnum))]
        public Category Category { get; set; }

        public int Ranking { get; set; }
        public bool? Available { get; set; }
        public double Rating { get; set; }
    }
}
