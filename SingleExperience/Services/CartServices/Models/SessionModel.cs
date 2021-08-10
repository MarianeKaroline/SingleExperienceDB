using SingleExperience.Entities;
using System.Collections.Generic;

namespace SingleExperience.Services.CartServices.Models
{
    class SessionModel
    {
        public int CountProduct { get; set; }
        public string Session { get; set; }
        public List<ProductCart> CartMemory { get; set; }
    }
}
