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
        private ClientService clientService;
        private ProductService productService;
        private CartService cartService;

        public BoughtService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
            productService = new ProductService(context);
            cartService = new CartService(context);
            clientService = new ClientService(context);
        }
                 

        //Lista apenas as compras do usuário
        public List<Bought> List(string userId)
        {
            //Irá procurar a compra pelo cpf do cliente
            return context.Bought
                .Select(p => new Bought
                {
                    BoughtId = p.BoughtId,
                    TotalPrice = p.TotalPrice,
                    AddressId = p.AddressId,
                    PaymentEnum = p.PaymentEnum,
                    CardNumber = p.CardNumber,
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
                codeBought = clientService.ListCard(parameters.Session)
                    .Where(p => p.Number.ToString().Contains(addBought.CodeConfirmation))
                    .FirstOrDefault().Number
                    .ToString();

            var bought = new Entities.Bought()
            {
                TotalPrice = addBought.TotalPrice,
                AddressId = addBought.AddressId,
                PaymentEnum = addBought.Payment,
                CardNumber = codeBought,
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
                    BoughtId = context.Bought.OrderByDescending(j => j.BoughtId).FirstOrDefault().BoughtId
                };

                context.ProductBought.Add(ProductBought);
                context.SaveChanges();
            });
        }

        public void UpdateStatus(int boughtId, StatusBoughtEnum status)
        {
            var getBought = context.Bought.FirstOrDefault(i => i.BoughtId == boughtId);

            getBought.StatusBoughtEnum = status;

            context.Bought.Update(getBought);
            context.SaveChanges();            
        }


        //Listar as compras do cliente
        public List<BoughtModel> ClientBought(string session)
        {
            var client = clientService.GetEnjoyer(session);
            var address = clientService.ListAddress(session);
            var card = clientService.ListCard(session);
            var cart = cartService.GetCart(session);
            var itens = cartService.ListItens(cart.CartId);
            var listProducts = new List<BoughtModel>();

            var listBought = List(session);

            if (listBought.Count > 0)
            {
                listBought.ForEach(i =>
                {
                    var boughtModel = new BoughtModel();
                    boughtModel.Itens = new List<ProductBoughtModel>();

                    boughtModel.ClientName = client.Name;
                    var aux = address
                    .FirstOrDefault(j => j.AddressId == i.AddressId);

                    boughtModel.Cep = aux.PostCode;
                    boughtModel.Street = aux.Street;
                    boughtModel.Number = aux.Number;
                    boughtModel.City = aux.City;
                    boughtModel.State = aux.State;

                    boughtModel.BoughtId = i.BoughtId;
                    boughtModel.paymentMethod = (PaymentEnum)i.PaymentEnum;

                    if (i.PaymentEnum == PaymentEnum.CreditCard)
                        card
                        .Where(j => j.Number.ToString().Contains(i.CardNumber))
                        .ToList()
                        .ForEach(k =>
                        {
                            boughtModel.NumberCard = k.Number.ToString();
                        });

                    boughtModel.TotalPrice = i.TotalPrice;
                    boughtModel.DateBought = i.DateBought;
                    boughtModel.StatusId = i.StatusBoughtEnum;


                    ListProductBought(i.BoughtId)
                    .ToList()
                    .ForEach(j =>
                    {
                        var product = new ProductBoughtModel();

                        product.ProductId = j.ProductId;
                        product.ProductName = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Name;
                        product.Amount = j.Amount;
                        product.Price = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Price;

                        boughtModel.Itens.Add(product);
                    });

                    listProducts.Add(boughtModel);
                });
            }

            return listProducts;
        }

        //Verifica se o número que o cliente digitou está correto
        public bool HasBought(int boughtId)
        {
            return context.Bought.Any(i => i.BoughtId == boughtId);
        }

        //Verifica se cliente já cadastrou algum endereço
        public bool HasAddress(string session)
        {
            return context.Address.Any(i => i.Cpf == session);
        }
    }
}
