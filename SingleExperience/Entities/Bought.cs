using SingleExperience.Entities.Enums;
using SingleExperience.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SingleExperience.Entities
{
    public class Bought
    {
        [Key]
        public int BoughtId { get; set; }        
        public double TotalPrice { get; set; }

        //FK - Address
        public int AddressId { get; set; }
        [ForeignKey(nameof(AddressId))]
        public Address Address { get; set; }

        //FK - Payment
        [Column("PaymentId")]
        public PaymentEnum PaymentEnum { get; set; }
        [ForeignKey(nameof(PaymentEnum))]
        public Payment Payment { get; set; }

        public string CodeBought { get; set; }
        public string Cpf { get; set; }
        [ForeignKey(nameof(Cpf))]
        public User User { get; set; }

        //FK - Status Bought
        [Column("StatusBoughtId")]
        public StatusBoughtEnum StatusBoughtEnum { get; set; }
        [ForeignKey(nameof(StatusBoughtEnum))]
        public StatusBought StatusBought { get; set; }

        public DateTime DateBought { get; set; }
    }
}
