using SingleExperience.Services.ProductServices.Model;
using SingleExperience.Entities.DB;
using SingleExperience.Entities;
using System.Collections.Generic;
using System.Linq;
using SingleExperience.Services.BoughtServices.Models;
using SingleExperience.Services.ProductServices.Models;
using SingleExperience.Enums;

namespace SingleExperience.Services.ProductServices
{
    public class ProductService
    {
        protected readonly SingleExperience.Context.SingleExperience context;

        public ProductService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
        }

        public ProductService()
        {
        }

        //Lê o arquivo CSV Produtos
        public List<Product> List()
        {
            return context.Product
                .Skip(1)
                .Select(i => new Product()
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Detail = i.Detail,
                    Amount = i.Amount,
                    CategoryEnum = i.CategoryEnum,
                    Ranking = i.Ranking,
                    Available = i.Available,
                    Rating = i.Rating
                })
                .ToList();
        }

        public List<ListProductsModel> ListAllProducts()
        {
            return List()
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
            return context.Product
                .Where(p => p.Available == true && p.ProductId == productId)
                .Select(i => new ProductSelectedModel()
                {
                    Rating = i.Rating,
                    CategoryId = i.CategoryEnum,
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Amount = i.Amount,
                    Detail = i.Detail
                })
                .FirstOrDefault();
        }

        //Se existe o produto com o id que o usuário digitou
        public bool HasProduct(int code)
        {
            return context.Product.Single(i => i.ProductId == code) != null;
        }

        //Diminui a quantidade do estoque quando a compra é confirmada pelo funcionário
        public bool Confirm(ProductBoughtModel product)
        {
            var teste = context.Product.FirstOrDefault(i => i.ProductId == product.ProductId);

            var model = new Entities.Product()
            {
                ProductId = teste.ProductId,
                Name = teste.Name,
                Price = teste.Price,
                Detail = teste.Detail,
                Amount = product.Amount,
                CategoryEnum = teste.CategoryEnum,
                Ranking = teste.Ranking,
                Available = teste.Available,
                Rating = teste.Rating
            };

            context.Product.Update(model);
            context.SaveChanges();

            return true;
        }

        //Deixando o produto disponivel ou indisponivel
        public bool EditAvailable(int productId, bool available)
        {
            var teste = context.Product.FirstOrDefault(i => i.ProductId == productId);

            var model = new Entities.Product()
            {
                ProductId = teste.ProductId,
                Name = teste.Name,
                Price = teste.Price,
                Detail = teste.Detail,
                Amount = teste.Amount,
                CategoryEnum = teste.CategoryEnum,
                Ranking = teste.Ranking,
                Available = available,
                Rating = teste.Rating
            };

            context.Product.Update(model);
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
