using Microsoft.EntityFrameworkCore;
using SingleExperience.Entities;

namespace SingleExperience.Context
{
    public class SingleExperience : DbContext
    {
        public DbSet<User> Enjoyer { get; set; }
        public DbSet<AccessEmployee> AccessEmployee { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<CreditCard> CreditCard { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<ProductCart> ProductCart { get; set; }
        public DbSet<StatusProductCart> StatusProductCart { get; set; }
        public DbSet<Bought> Bought { get; set; }
        public DbSet<ProductBought> ProductBought { get; set; }
        public DbSet<StatusBought> StatusBought { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=mariane.santos;Data Source=localhost");
        }
    }
}
