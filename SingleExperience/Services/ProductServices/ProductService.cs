using SingleExperience.Enums;
using SingleExperience.Services.BoughtServices.Models;
using SingleExperience.Services.ProductServices.Model;
using SingleExperience.Services.ProductServices.Models;
using System.Collections.Generic;
using System.Linq;

namespace SingleExperience.Services.ProductServices
{
    public class ProductService
    {
        protected readonly SingleExperience.Context.SingleExperience context;

        public ProductService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
        }

        public List<ListProductsModel> ListAllProducts()
        {
            return context.Product
                .Select(i => new ListProductsModel()
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Amount = i.Amount,
                    CategoryId = i.CategoryEnum,
                    Ranking = i.Ranking,
                    Available = i.Available
                })
                .ToList();
        }

        //Listar Produtos Home
        public List<BestSellingModel> ListProducts()
        {
            return context.Product
                .Where(p => p.Available == true)
                .Take(5)
                .OrderByDescending(p => p.Ranking)
                .Select(i => new BestSellingModel()
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Available = i.Available,
                    Ranking = i.Ranking
                })
                .ToList();
        }

        //Listar Produtos Categoria
        public List<CategoryModel> ListProductCategory(CategoryEnum categoryId)
        {
            return context.Product
                .Where(p => p.Available == true && p.CategoryEnum == categoryId)
                .Select(i => new CategoryModel()
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    CategoryId = i.CategoryEnum,
                    Available = i.Available
                })
                .ToList();
        }

        //Listar Produto Selecionado
        public ProductSelectedModel SelectedProduct(int productId)
        {
            var teste = context.Product
             .Where(p => p.Available == true && p.ProductId == productId)
             .Select(i => new ProductSelectedModel()
             {
                 CategoryId = i.CategoryEnum,
                 ProductId = i.ProductId,
                 Name = i.Name,
                 Amount = i.Amount,
                 Detail = i.Detail,
                 Price = i.Price,
                 Rating = i.Rating
             })
             .FirstOrDefault();

            return teste;
        }

        //Se existe o produto com o id que o usuário digitou
        public bool HasProduct(int code)
        {
            return context.Product.Any(i => i.ProductId == code);
        }

        //Diminui a quantidade do estoque quando a compra é confirmada pelo funcionário
        public bool Confirm(List<ProductBoughtModel> products)
        {
            products.ForEach(j =>
            {
                var product = context.Product
                    .Where(i => i.ProductId == j.ProductId)
                    .FirstOrDefault();

                product.Amount = j.Amount;

                context.Product.Update(product);
                context.SaveChanges();
            });

            return true;
        }

        //Deixando o produto disponivel ou indisponivel
        public bool EditAvailable(int productId, bool available)
        {
            var product = context.Product
                .FirstOrDefault(i => i.ProductId == productId && i.Available != available);

            product.Available = available;

            context.Product.Update(product);
            context.SaveChanges();

            return true;
        }

        public void AddNewProducts(AddNewProductModel newProduct)
        {
            var model = new Entities.Product()
            {
                Name = newProduct.Name,
                Price = newProduct.Price,
                Detail = newProduct.Detail,
                Amount = newProduct.Amount,
                CategoryEnum = newProduct.CategoryId,
                Ranking = newProduct.Ranking,
                Available = newProduct.Available,
                Rating = newProduct.Rating
            };

            context.Product.Add(model);
            context.SaveChanges();
        }
    }
}
