using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleExperience.Entities
{
    class CreditCard
    {
        [Key]
        public int CardId { get; set; }
        public long CardNumber { get; set; }
        public string Name { get; set; }
        public DateTime ShelfLife { get; set; }
        public int CVV { get; set; }

        //FK - Cpf
        public string Cpf { get; set; }
        [ForeignKey(nameof(Cpf))]
        public User User { get; set; }
    }
}
