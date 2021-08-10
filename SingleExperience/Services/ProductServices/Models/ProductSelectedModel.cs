using SingleExperience.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SingleExperience.Services.ProductServices.Model
{
    public class ProductSelectedModel
    {
        public int ProductId { get; set; }
        public double Rating { get; set; }
        public string Name { get; set; }
        public CategoryEnum CategoryId { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public string Detail { get; set; }
    }
}
