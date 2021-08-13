﻿using SingleExperience.Entities;
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
        protected readonly Context.SingleExperience context;
        private ClientService clientService;
        private ProductService productService;
        private CartService cartService;

        public BoughtService(Context.SingleExperience context)
        {
            this.context = context;
            productService = new ProductService(context);
            cartService = new CartService(context);
            clientService = new ClientService(context);
        }

        public List<Bought> List(string cpf)
        {
            //Irá procurar a compra pelo cpf do cliente
            return context.Bought
                .Select(p => new Bought
                {
                    BoughtId = p.BoughtId,
                    TotalPrice = p.TotalPrice,
                    AddressId = p.AddressId,
                    PaymentEnum = p.PaymentEnum,
                    CreditCardId = p.CreditCardId,
                    Cpf = p.Cpf,
                    StatusBoughtEnum = p.StatusBoughtEnum,
                    DateBought = p.DateBought
                })
                .Where(p => p.Cpf == cpf)
                .ToList();
        }

        public List<ProductBought> ListProduct(int boughtId)
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

        public void Add(string cpf, AddBoughtModel addBought)
        {
            var codeBought = 0;
            StatusBoughtEnum statusBought = 0;

            if (addBought.Payment == PaymentEnum.BankSlip)
                statusBought = StatusBoughtEnum.PagamentoPendente;
            else
                statusBought = StatusBoughtEnum.ConfirmacaoPendente;

            if (addBought.Payment == PaymentEnum.CreditCard)
                codeBought = clientService.ListCard(cpf)
                    .Where(p => p.CreditCardId == addBought.CreditCardId)
                    .FirstOrDefault().CreditCardId;

            //Adiciona compra
            var bought = new Entities.Bought()
            {
                TotalPrice = addBought.TotalPrice,
                AddressId = addBought.AddressId,
                PaymentEnum = addBought.Payment,
                CreditCardId = codeBought,
                Cpf = cpf,
                StatusBoughtEnum = statusBought,
                DateBought = DateTime.Now
            };

            context.Bought.Add(bought);
            //context.SaveChanges(); ---Teste

            AddProduct(cpf);
        }

        public void AddProduct(string cpf)
        {
            var getCart = cartService.Get(cpf);
            var listItens = new List<ProductCart>();

            //Adiciona na lista os produtos que estão ativos no carrinho
            listItens.Add(cartService.ListItens(getCart.CartId)
                .Where(i => i.StatusProductEnum == StatusProductEnum.Ativo)
                .FirstOrDefault());

            listItens.ForEach(i =>
            {
                //Adiciona itens do carrinho na tabela ProductBought
                var ProductBought = new Entities.ProductBought()
                {
                    ProductId = i.ProductId,
                    Amount = i.Amount,
                    BoughtId = context.Bought.OrderByDescending(j => j.BoughtId).FirstOrDefault().BoughtId
                };

                context.ProductBought.Add(ProductBought);
            });

            context.SaveChanges();
        }

        public void UpdateStatus(int boughtId, StatusBoughtEnum status)
        {
            var getBought = context.Bought.FirstOrDefault(i => i.BoughtId == boughtId);

            getBought.StatusBoughtEnum = status;

            context.Bought.Update(getBought);
            context.SaveChanges();
        }

        public PreviewBoughtModel PreviewBoughts(SessionModel parameters, BuyModel bought, int addressId)
        {
            var preview = new PreviewBoughtModel();
            var client = clientService.GetUser(bought.Session);
            var address = clientService.ListAddress(parameters.Session);
            var card = clientService.ListCard(bought.Session);
            var cart = cartService.Get(bought.Session);
            var itens = cartService.ListItens(cart.CartId);
            var listProducts = new List<ProductCartModel>();

            //Pega alguns atributos do cliente
            preview.FullName = client.Name;
            preview.Phone = client.Phone;

            //Pega alguns atributos do endereço
            var aux = address
                .FirstOrDefault(i => i.AddressId == addressId);

            preview.Cep = aux.PostCode;
            preview.Street = aux.Street;
            preview.Number = aux.Number;
            preview.City = aux.City;
            preview.State = aux.State;

            preview.Method = bought.Method;

            if (bought.Method == PaymentEnum.CreditCard)
            {
                card
                    .Where(i => i.Number.ToString().Contains(bought.Confirmation))
                    .ToList()
                    .ForEach(i =>
                    {
                        preview.NumberCard = i.Number.ToString();
                    });
            }
            else if (bought.Method == PaymentEnum.BankSlip)
            {
                var a = bought.Confirmation.Length;
                preview.Code = bought.Confirmation;
            }
            else
            {
                preview.Pix = bought.Confirmation;
            }

            if (bought.Ids.Count > 0)
            {
                bought.Ids.ForEach(i =>
                {
                    listProducts.Add(cartService.ShowProducts(parameters, bought.Status)
                                    .Where(j => j.ProductId == i)
                                    .FirstOrDefault());
                });
                preview.Itens = listProducts;
            }
            else
            {
                preview.Itens = cartService.ShowProducts(parameters, bought.Status);
            }

            return preview;
        }


        public List<BoughtModel> Show(string cpf)
        {
            var client = clientService.GetUser(cpf);
            var address = clientService.ListAddress(cpf);
            var card = clientService.ListCard(cpf);
            var cart = cartService.Get(cpf);
            var itens = cartService.ListItens(cart.CartId);
            var listProducts = new List<BoughtModel>();

            var listBought = List(cpf);

            //Listar as compras do cliente
            if (listBought.Count > 0)
            {
                listBought.ForEach(i =>
                {
                    var boughtModel = new BoughtModel();
                    boughtModel.Itens = new List<ProductBoughtModel>();

                    boughtModel.ClientName = client.Name;
                    var aux = address.FirstOrDefault(j => j.AddressId == i.AddressId);

                    boughtModel.Cep = aux.PostCode;
                    boughtModel.Street = aux.Street;
                    boughtModel.Number = aux.Number;
                    boughtModel.City = aux.City;
                    boughtModel.State = aux.State;

                    boughtModel.BoughtId = i.BoughtId;
                    boughtModel.paymentMethod = (PaymentEnum)i.PaymentEnum;

                    if (i.PaymentEnum == PaymentEnum.CreditCard)
                        boughtModel.NumberCard = card.FirstOrDefault(j => j.CreditCardId == i.CreditCardId).Number;

                    boughtModel.TotalPrice = i.TotalPrice;
                    boughtModel.DateBought = i.DateBought;
                    boughtModel.StatusId = i.StatusBoughtEnum;


                    boughtModel.Itens = ListProduct(i.BoughtId)
                        .Select(j => new ProductBoughtModel()
                        {
                            ProductId = j.ProductId,
                            ProductName = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Name,
                            Amount = j.Amount,
                            Price = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Price,
                            BoughtId = j.BoughtId
                        })
                        .ToList();

                    listProducts.Add(boughtModel);
                });
            }

            return listProducts;
        }

        public bool HasBought(int boughtId)
        {
            //Verifica se o número da compra que o cliente digitou está correto
            return context.Bought.Any(i => i.BoughtId == boughtId);
        }        

        public List<BoughtModel> ListAll()
        {
            var listProducts = new List<BoughtModel>();
            var listBought = context.Bought.ToList();

            //Lista todas as compras
            listBought.ForEach(i =>
            {
                var client = clientService.GetUser(i.Cpf);
                var address = clientService.ListAddress(i.Cpf);
                var card = clientService.ListCard(i.Cpf);
                var cart = cartService.Get(i.Cpf);
                var itens = cartService.ListItens(cart.CartId);
                var boughtModel = new BoughtModel();
                boughtModel.Itens = new List<ProductBoughtModel>();

                boughtModel.ClientName = client.Name;
                var aux = address.FirstOrDefault(j => j.AddressId == i.AddressId);

                boughtModel.Cep = aux.PostCode;
                boughtModel.Street = aux.Street;
                boughtModel.Number = aux.Number;
                boughtModel.City = aux.City;
                boughtModel.State = aux.State;

                boughtModel.BoughtId = i.BoughtId;
                boughtModel.paymentMethod = (PaymentEnum)i.PaymentEnum;

                if (i.PaymentEnum == PaymentEnum.CreditCard)
                    boughtModel.NumberCard = card.FirstOrDefault(j => j.CreditCardId == i.CreditCardId).Number;

                boughtModel.TotalPrice = i.TotalPrice;
                boughtModel.DateBought = i.DateBought;
                boughtModel.StatusId = i.StatusBoughtEnum;

                boughtModel.Itens = ListProduct(i.BoughtId)
                    .Select(j => new ProductBoughtModel()
                    {
                        ProductId = j.ProductId,
                        ProductName = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Name,
                        Amount = j.Amount,
                        Price = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Price,
                        BoughtId = j.BoughtId
                    })
                    .ToList();

                listProducts.Add(boughtModel);
            });

            return listProducts;
        }

        public List<BoughtModel> BoughtPendent(StatusBoughtEnum status)
        {
            return ListAll().Where(i => i.StatusId == status).ToList();
        }
    }
}
