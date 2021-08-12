using SingleExperience.Entities;
using SingleExperience.Entities.Enums;
using SingleExperience.Enums;
using SingleExperience.Services.BoughtServices.Models;
using SingleExperience.Services.CartServices;
using SingleExperience.Services.CartServices.Models;
using SingleExperience.Services.ClientServices;
using SingleExperience.Services.ProductServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleExperience.Services.BoughtServices
{
    public class BoughtService
    {
        protected readonly SingleExperience.Context.SingleExperience context;
        private ClientService clientDB;
        private ProductService productService;
        private CartService cartService;

        public BoughtService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
            productService = new ProductService(context);
            cartService = new CartService(context);
            clientDB = new ClientService(context);
        }
                 

        //Lista apenas as compras do usuário
        public List<Bought> List(string userId)
        {
            //Irá procurar a compra pelo cpf do cliente
            return context.Bought
                .Select(p => new Bought
                {
                    TotalPrice = p.TotalPrice,
                    AddressId = p.AddressId,
                    PaymentEnum = p.PaymentEnum,
                    CodeBought = p.CodeBought,
                    Cpf = p.Cpf,
                    StatusBoughtEnum = p.StatusBoughtEnum,
                    DateBought = p.DateBought
                })
                .Where(p => p.Cpf == userId)
                .ToList();
        }

        //Lista os produtos comprados
        public List<ProductBought> ListProductBought(int boughtId)
        {
            //Irá procurar o os produtos da compra pela compra id
            return context.ProductBought
                .Select(p => new ProductBought
                {
                    ProductId = p.ProductId,
                    Amount = p.Amount,
                    BoughtId = p.BoughtId,
                })
                .Where(p => p.BoughtId == boughtId)
                .ToList();
        }

        //Adiciona os produtos nas tabelas de compras
        public void AddBought(SessionModel parameters, AddBoughtModel addBought)
        {
            var getCart = cartService.GetCart(parameters.Session);
            var listItens = new List<ProductCart>();
            string codeBought = ""; //Esse code Bought é o número do cartão, ou o gid gerado para boleto e pix
            StatusBoughtEnum statusBought = 0;

            //Verifica se o pagamento foi feito com boleto, para transformar o status do produto em Pagamento Pendente
            if (addBought.Payment == PaymentEnum.BankSlip)
                statusBought = StatusBoughtEnum.PagamentoPendente;
            else
                statusBought = StatusBoughtEnum.ConfirmacaoPendente;

            if (addBought.Payment == PaymentEnum.CreditCard)
                codeBought = clientDB.ListCard(parameters.Session)
                    .Where(p => p.CardNumber.ToString().Contains(addBought.CodeConfirmation))
                    .FirstOrDefault().CardNumber
                    .ToString();
            else
                codeBought = addBought.CodeConfirmation;

            var bought = new Entities.Bought()
            {
                TotalPrice = addBought.TotalPrice,
                AddressId = addBought.AddressId,
                PaymentEnum = addBought.Payment,
                CodeBought = codeBought,
                Cpf = parameters.Session,
                StatusBoughtEnum = statusBought,
                DateBought = DateTime.Now
            };

            context.Bought.Add(bought);
            context.SaveChanges();

            //Pega os dados do último produto comprado
            addBought.BuyProducts.ForEach(j =>
            {
                listItens.Add(cartService.ListItens(getCart.CartId)
                    .Where(i =>
                        i.StatusProductEnum == StatusProductEnum.Comprado &&
                        i.ProductId == j.ProductId)
                    .FirstOrDefault());
            });


            listItens.ForEach(i =>
            {
                var ProductBought = new Entities.ProductBought()
                {
                    ProductId = i.ProductId,
                    Amount = i.Amount,
                    BoughtId = context.Bought.LastOrDefault().BoughtId
                };

                context.ProductBought.Add(ProductBought);
                context.SaveChanges();
            });
        }

        public void UpdateStatus(int boughtId, StatusBoughtEnum status)
        {
            var getBought = context.Bought.FirstOrDefault(i => i.BoughtId == boughtId);

            var bought = new Entities.Bought()
            {
                TotalPrice = getBought.TotalPrice,
                AddressId = getBought.AddressId,
                PaymentEnum = getBought.PaymentEnum,
                CodeBought = getBought.CodeBought,
                Cpf = getBought.Cpf,
                StatusBoughtEnum = status,
                DateBought = getBought.DateBought
            };

            context.Bought.Update(bought);
            context.SaveChanges();            
        }


        //Listar as compras do cliente
        public List<BoughtModel> ClientBought(string session)
        {
            var client = clientDB.GetEnjoyer(session);
            var address = clientDB.ListAddress(session);
            var card = clientDB.ListCard(session);
            var cart = cartService.GetCart(session);
            var itens = cartService.ListItens(cart.CartId);
            var listProducts = new List<BoughtModel>();

            var listBought = List(session);

            listBought.ForEach(i =>
            {
                var boughtModel = new BoughtModel();
                boughtModel.Itens = new List<ProductBoughtModel>();

                boughtModel.ClientName = client.FullName;
                var aux = address
                .FirstOrDefault(j => j.AddressId == i.AddressId);

                boughtModel.Cep = aux.Cep;
                boughtModel.Street = aux.Street;
                boughtModel.Number = aux.Number;
                boughtModel.City = aux.City;
                boughtModel.State = aux.State;

                boughtModel.BoughtId = i.BoughtId;
                boughtModel.paymentMethod = (PaymentEnum)i.PaymentEnum;

                if (i.PaymentEnum == PaymentEnum.CreditCard)
                    card
                    .Where(j => j.CardNumber.ToString().Contains(i.CodeBought))
                    .ToList()
                    .ForEach(k =>
                    {
                        boughtModel.NumberCard = k.CardNumber.ToString();
                    });
                else if (i.PaymentEnum == PaymentEnum.BankSlip)
                    boughtModel.Code = i.CodeBought;
                else
                    boughtModel.Pix = i.CodeBought;

                boughtModel.TotalPrice = i.TotalPrice;
                boughtModel.DateBought = i.DateBought;
                boughtModel.StatusId = i.StatusBoughtEnum;


                ListProductBought(i.BoughtId)
                .ToList()
                .ForEach(j =>
                {
                    var product = new ProductBoughtModel();

                    product.ProductId = j.ProductId;
                    product.ProductName = productService.ListProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Name;
                    product.Amount = j.Amount;
                    product.Price = productService.ListProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Price;

                    boughtModel.Itens.Add(product);
                });

                listProducts.Add(boughtModel);
            });

            return listProducts;
        }

        //Verifica se o número que o cliente digitou está correto
        public bool HasBought(int boughtId)
        {
            return context.Bought.FirstOrDefault(i => i.BoughtId == boughtId) != null;
        }

        //Verifica se cliente já cadastrou algum endereço
        public bool HasAddress(string session)
        {
            return context.Bought.FirstOrDefault(i => i.Cpf == session) != null;
        }
    }
}
