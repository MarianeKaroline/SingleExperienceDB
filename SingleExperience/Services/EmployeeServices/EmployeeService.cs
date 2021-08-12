using SingleExperience.Entities;
using SingleExperience.Entities.Enums;
using SingleExperience.Enums;
using SingleExperience.Services.BoughtServices;
using SingleExperience.Services.BoughtServices.Models;
using SingleExperience.Services.CartServices;
using SingleExperience.Services.ClientServices;
using SingleExperience.Services.ClientServices.Models;
using SingleExperience.Services.EmployeeServices.Models;
using SingleExperience.Services.UserServices;
using SingleExperience.Services.ProductServices;
using System.Collections.Generic;
using System.Linq;

namespace SingleExperience.Services.EmployeeServices
{
    public class EmployeeService : UserService
    {
        protected readonly SingleExperience.Context.SingleExperience context;
        private BoughtService boughtDB;
        private CartService cartService;
        private ClientService clientService;
        private ProductService productService;

        public EmployeeService(SingleExperience.Context.SingleExperience context) : base(context)
        {
            this.context = context;
            boughtDB = new BoughtService(context);
            cartService = new CartService(context);
            productService = new ProductService(context);
            clientService = new ClientService(context);
        }

        //Lista o acesso do funcionário
        public AccessEmployee Access(string cpf)
        {
            return context.AccessEmployee
                .Select(i => new AccessEmployee
                {
                    Cpf = i.Cpf,
                    AccessInventory = i.AccessInventory,
                    AccessRegister = i.AccessRegister
                })
                .FirstOrDefault(i => i.Cpf == cpf);
        }

        //Cadastra funcionário
        public bool Register(SignUpModel employee)
        {
            var existEmployee = GetEnjoyer(employee.Cpf);
            if (existEmployee == null)
            {
                SignUp(employee);

                var access = new AccessEmployee()
                {
                    Cpf = employee.Cpf,
                    AccessInventory = employee.AccessInventory,
                    AccessRegister = employee.AccessRegister
                };

                context.AccessEmployee.Add(access);
                context.SaveChanges();
            }

            return existEmployee == null;
        }


        public List<BoughtModel> Bought()
        {
            var listProducts = new List<BoughtModel>();
            var listBought = context.Bought.ToList();

            listBought.ForEach(i =>
            {
                var client = clientService.GetEnjoyer(i.Cpf);
                var address = clientService.ListAddress(i.Cpf);
                var card = clientService.ListCard(i.Cpf);
                var cart = cartService.GetCart(i.Cpf);
                var itens = cartService.ListItens(cart.CartId);
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
                {
                    card
                    .Where(j => j.Number.ToString().Contains(i.CardNumber))
                    .ToList()
                    .ForEach(k =>
                    {
                        boughtModel.NumberCard = k.Number.ToString();
                    });
                }
                else if (i.PaymentEnum == PaymentEnum.BankSlip)
                {
                    boughtModel.Code = i.CardNumber;
                }
                else
                {
                    boughtModel.Pix = i.CardNumber;
                }
                boughtModel.TotalPrice = i.TotalPrice;
                boughtModel.DateBought = i.DateBought;
                boughtModel.StatusId = i.StatusBoughtEnum;

                boughtDB.ListProductBought(i.BoughtId)
                .ToList()
                .ForEach(j =>
                {
                    var product = new ProductBoughtModel();

                    product.ProductId = j.ProductId;
                    product.ProductName = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Name;
                    product.Amount = j.Amount;
                    product.Price = productService.ListAllProducts().FirstOrDefault(i => i.ProductId == j.ProductId).Price;
                    product.BoughtId = j.BoughtId;

                    boughtModel.Itens.Add(product);
                });

                listProducts.Add(boughtModel);
            });

            return listProducts;
        }

        public List<BoughtModel> BoughtPendent(StatusBoughtEnum status)
        {
            return Bought().Where(i => i.StatusId == status).ToList();
        }

        public List<RegisteredModel> listEmployee()
        {
            return context.Enjoyer
                     .Where(i => i.Employee == true)
                     .Select(i => new RegisteredModel()
                     {
                         Cpf = i.Cpf,
                         FullName = i.Name,
                         Email = i.Email,
                         AccessInventory = Access(i.Cpf).AccessInventory,
                         RegisterEmployee = Access(i.Cpf).AccessRegister
                     })
                     .ToList();
        }
    }
}
