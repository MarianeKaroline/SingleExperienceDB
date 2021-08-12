using SingleExperience.Services.CartServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SingleExperience.Enums;
using SingleExperience.Entities;
using SingleExperience.Services.ProductServices;
using SingleExperience.Services.ClientServices;

namespace SingleExperience.Services.CartServices
{
    public class CartService
    {
        protected readonly SingleExperience.Context.SingleExperience context;
        private ProductService productService;
        private ClientService clientService;

        public CartService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
            productService = new ProductService(context);
            clientService = new ClientService(context);
        }


        public CartService()
        {
        }

        //Read Datas

        //Cart
        public Cart GetCart(string userId)
        {
            //Irá procurar o carrinho pelo userId
            return context.Cart
                .Select(p => new Cart
                {
                    CartId = p.CartId,
                    Cpf = p.Cpf,
                    DateCreated = p.DateCreated
                })
                .FirstOrDefault(p => p.Cpf == userId);
        }

        //Itens Cart
        public List<ProductCart> ListItens(int cartId)
        {
            //Retorna a lista de produtos do carrinho
            return context.ProductCart
                .Select(i => new ProductCart
                {
                    ItemCartId = i.ItemCartId,
                    ProductId = i.ProductId,
                    CartId = i.CartId,
                    Amount = i.Amount,
                    StatusProductEnum = i.StatusProductEnum
                })
                .Where(i => i.CartId == cartId)
                .ToList();
        }

        //Total do Carrinho
        public TotalCartModel TotalCart(SessionModel parameters)
        {
            var itens = ShowCart(parameters, StatusProductEnum.Ativo);
            var total = new TotalCartModel();

            if (parameters.Session.Length == 11)
            {
                total.TotalAmount = itens
                    .Where(item => item.StatusId == StatusProductEnum.Ativo)
                    .Sum(item => item.Amount);
                total.TotalPrice = itens
                    .Where(item => item.StatusId == StatusProductEnum.Ativo)
                    .Sum(item => item.Price * item.Amount);
            }
            else
            {
                if (parameters.CartMemory.Count == 0)
                {
                    total.TotalAmount = 0;
                    total.TotalPrice = 0;
                }
                else
                {
                    total.TotalAmount = parameters.CartMemory.Sum(item => item.Amount);
                    total.TotalPrice = parameters.CartMemory.Sum(item => productService.ListProducts().FirstOrDefault(i => i.ProductId == item.ProductId).Price * item.Amount);
                }
            }

            return total;
        }


        //Add Datas

        //Cart
        public int AddCart(SessionModel parameters)
        {
            var currentCart = GetCart(parameters.Session);
            var cartId = 0;

            //Criar Carrinho
            if (currentCart == null)
            {
                var cart = new Entities.Cart()
                {
                    Cpf = parameters.Session,
                    DateCreated = DateTime.Now
                };

                context.Cart.Add(cart);
                context.SaveChanges();

                cartId = context.Cart.LastOrDefault().CartId;
            }
            else
            {
                cartId = currentCart.CartId;
            }

            return cartId;
        }

        //itens Cart
        public void AddItemCart(SessionModel parameters, CartModel cartModel)
        {
            var cartId = AddCart(parameters);
            var exist = ExistItem(parameters, cartModel);

            if (parameters.CartMemory.Count > 0)
            {
                PassItens(parameters);
                exist = true;
            }

            if (!exist)
            {
                var item = new Entities.ProductCart()
                {
                    ProductId = cartModel.ProductId,
                    CartId = cartId,
                    Amount = 1,
                    StatusProductEnum = cartModel.StatusId
                };

                context.ProductCart.Add(item);
                context.SaveChanges();
            }
        }

        //Pass memory's itens to cart 
        public void PassItens(SessionModel parameters)
        {
            var linesCart = new List<string>();
            var exist = false;

            //Verify if cliente already has a cart
            var cartId = AddCart(parameters);
            var listItensCart = ListItens(cartId);

            //Verify if product is already in the cart
            if (listItensCart.Count() > 0)
            {
                parameters.CartMemory.ForEach(i =>
                {
                    CartModel cartModel = new CartModel()
                    {
                        ProductId = i.ProductId,
                        UserId = parameters.Session,
                        Name = i.Product.Name,
                        CategoryId = i.Product.CategoryEnum,
                        StatusId = i.StatusProductEnum,
                        Price = i.Product.Price
                    };

                    exist = ExistItem(parameters, cartModel);
                });
            }

            //Passa the product to cart
            if (!exist)
            {
                parameters.CartMemory.ForEach(i =>
                {
                    var item = new Entities.ProductCart()
                    {
                        ProductId = i.ProductId,
                        CartId = cartId,
                        Amount = 1,
                        StatusProductEnum = i.StatusProductEnum
                    };

                    context.ProductCart.Add(item);
                    context.SaveChanges();
                });
            }
        }


        //Update datas

        //Remove a cart's item
        public void RemoveItem(int productId, string session, SessionModel parameters)
        {
            var getCart = GetCart(session);
            var getItem = ListItens(getCart.CartId).FirstOrDefault(i => i.ProductId == productId);
            var sum = 0;
            var count = 0;

            if (session.Length == 11)
            {
                if (getItem.Amount > 1 && count == 0)
                {
                    sum = getItem.Amount - 1;
                    EditAmount(productId, session, sum);
                    count++;
                }
                else if (getItem.Amount == 1)
                {
                    EditStatusProduct(productId, session, StatusProductEnum.Inativo);
                }
            }
            else
            {
                var aux = 0;
                parameters.CartMemory.ForEach(i =>
                {
                    if (i.ProductId == productId && i.Amount > 1)
                    {
                        i.Amount -= 1;
                    }
                    else if (i.ProductId == productId && i.Amount == 1)
                    {
                        aux++;
                    }
                });

                if (aux > 0)
                {
                    parameters.CartMemory.RemoveAll(x => x.ProductId == productId);
                }

            }

        }

        //Edit product's status
        public void EditStatusProduct(int productId, string session, StatusProductEnum status)
        {
            var getItem = ListItens(GetCart(session).CartId).FirstOrDefault(i => i.ProductId == productId);
            var auxAmount = 0;

            if (status == StatusProductEnum.Ativo)
            {
                auxAmount = 1;
            }
            else
            {
                auxAmount = getItem.Amount;
            }

            var item = new Entities.ProductCart()
            {
                ProductId = productId,
                CartId = GetCart(session).CartId,
                Amount = auxAmount,
                StatusProductEnum = status
            };

            context.ProductCart.Update(item);
            context.SaveChanges();
        }

        //Edit product's amount
        public void EditAmount(int productId, string session, int sub)
        {
            var getItem = ListItens(GetCart(session).CartId).FirstOrDefault(i => i.ProductId == productId);
            var lines = new List<string>();

            var item = new Entities.ProductCart()
            {
                ProductId = productId,
                CartId = GetCart(session).CartId,
                Amount = sub,
                StatusProductEnum = getItem.StatusProductEnum
            };

            context.ProductCart.Update(item);
            context.SaveChanges();
        }

        //Preview Bought's datas 
        public PreviewBoughtModel PreviewBoughts(SessionModel parameters, BuyModel bought, int addressId)
        {
            var preview = new PreviewBoughtModel();
            var client = clientService.GetEnjoyer(bought.Session);
            var address = clientService.ListAddress(parameters.Session);
            var card = clientService.ListCard(bought.Session);
            var cart = GetCart(bought.Session);
            var itens = ListItens(cart.CartId);
            var listProducts = new List<ProductCartModel>();

            //Pega alguns atributos do cliente
            preview.FullName = client.FullName;
            preview.Phone = client.Phone;

            //Pega alguns atributos do endereço
            var aux = address
                .FirstOrDefault(i => i.AddressId == addressId);

            preview.Cep = aux.Cep;
            preview.Street = aux.Street;
            preview.Number = aux.Number;
            preview.City = aux.City;
            preview.State = aux.State;

            preview.Method = bought.Method;

            if (bought.Method == PaymentEnum.CreditCard)
            {
                card
                    .Where(i => i.CardNumber.ToString().Contains(bought.Confirmation))
                    .ToList()
                    .ForEach(i =>
                    {
                        preview.NumberCard = i.CardNumber.ToString();
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
                    listProducts.Add(ShowCart(parameters, bought.Status)
                                    .Where(j => j.ProductId == i)
                                    .FirstOrDefault());
                });
                preview.Itens = listProducts;
            }
            else
            {
                preview.Itens = ShowCart(parameters, bought.Status);
            }

            return preview;
        }

        //Show Cart's Itens
        public List<ProductCartModel> ShowCart(SessionModel parameters, StatusProductEnum status)
        {
            var productService = new ProductService(context);
            var prod = new List<ProductCartModel>();

            if (parameters.Session.Length == 11)
            {
                var itensCart = ListItens(GetCart(parameters.Session).CartId);
                var product = productService.ListAllProducts();

                try
                {
                    prod = itensCart
                        .Where(i => i.StatusProductEnum == status)
                        .Select(j => new ProductCartModel()
                        {
                            ProductId = j.ProductId,
                            Name = product.FirstOrDefault(i => i.ProductId == j.ProductId).Name,
                            CategoryId = product.FirstOrDefault(i => i.ProductId == j.ProductId).CategoryId,
                            StatusId = j.StatusProductEnum,
                            Amount = j.Amount,
                            Price = product.FirstOrDefault(i => i.ProductId == j.ProductId).Price
                        })
                        .ToList();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Ocorreu um erro");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                prod = parameters.CartMemory
                        .Where(i => i.StatusProductEnum == status)
                        .Select(j => new ProductCartModel()
                        {
                            ProductId = j.ProductId,
                            StatusId = j.StatusProductEnum,
                            Amount = j.Amount
                        })
                        .ToList();
            }

            return prod;
        }

        //After to confirm the bought call the method to change de status
        public bool Buy(List<BuyProductModel> products, string session)
        {
            var buy = false;

            products.ForEach(i =>
            {
                EditStatusProduct(i.ProductId, session, i.Status);
                buy = true;
            });
            return buy;
        }

        //Verify if exist the item in the cart
        public bool ExistItem(SessionModel parameters, CartModel cartModel)
        {
            var cartId = AddCart(parameters);
            var listItensCart = ListItens(cartId);
            var exist = false;
            var sum = 1;

            listItensCart.ForEach(j =>
            {
                if (j.ProductId == cartModel.ProductId && j.StatusProductEnum != StatusProductEnum.Ativo)
                {
                    EditStatusProduct(cartModel.ProductId, cartModel.UserId, StatusProductEnum.Ativo);
                    exist = true;
                }
                else if (j.ProductId == cartModel.ProductId)
                {
                    sum += j.Amount;
                    EditAmount(cartModel.ProductId, cartModel.UserId, sum);
                    exist = true;
                }
            });

            return exist;
        }

        /*Memory*/
        //Pass the products to memory
        public List<ProductCart> AddItensMemory(CartModel cart, List<ProductCart> cartMemory)
        {
            //Verify if cartMemory is empty
            if (cartMemory == null)
            {
                cartMemory = new List<ProductCart>();
            }
            var sum = 1;

            //Verify if cart productId is different of zero
            if (cart.ProductId != 0)
            {
                var aux = cartMemory
                        .Where(i => i.ProductId == cart.ProductId)
                        .FirstOrDefault();

                if (aux == null)
                {
                    var item = new ProductCart()
                    {
                        ItemCartId = 1,
                        ProductId = cart.ProductId,
                        Amount = sum,
                        StatusProductEnum = cart.StatusId
                    };
                    cartMemory.Add(item);
                }
                else
                {
                    cartMemory.ForEach(i =>
                    {
                        i.ItemCartId = cartMemory.Count();
                        i.ProductId = cart.ProductId;
                        i.Amount += sum;
                        i.StatusProductEnum = cart.StatusId;
                    });
                }
            }

            return cartMemory;
        }
    }
}
