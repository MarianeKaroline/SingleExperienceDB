using SingleExperience.Entities;
using SingleExperience.Services.ClientServices.Models;
using SingleExperience.Services.UserServices;
using System.Collections.Generic;
using System.Linq;

namespace SingleExperience.Services.ClientServices
{
    public class ClientService : UserService
    {
        protected readonly SingleExperience.Context.SingleExperience context;

        public ClientService(SingleExperience.Context.SingleExperience context) : base(context)
        {
            this.context = context;
        }

        //Address
        public List<Address> ListAddress(string userId)
        {
            return context.Address
                .Select(i => new Address
                {
                    Cep = i.Cep,
                    Street = i.Street,
                    Number = i.Number,
                    City = i.City,
                    State = i.State,
                    Cpf = i.Cpf
                })
                .Where(i => i.Cpf == userId)
                .ToList();
        }

        //Card
        public List<CreditCard> ListCard(string userId)
        {
            return context.CreditCard
                    .Where(i => i.Cpf == userId)
                    .Select(i => new CreditCard
                    {
                        CardNumber = i.CardNumber,
                        Name = i.Name,
                        ShelfLife = i.ShelfLife,
                        CVV = i.CVV,
                        Cpf = i.Cpf
                    })
                    .ToList();
        }


        /* Cadastro */
        //Client
        public bool SignUpClient(SignUpModel client)
        {
            var existClient = GetEnjoyer(client.Cpf);

            if (existClient == null)
            {
                SignUp(client);
            }

            return existClient == null;
        }

        //Address
        public int AddAddress(string session, AddressModel addressModel)
        {
            var address = new Entities.Address()
            {
                Cep = addressModel.Cep,
                Street = addressModel.Street,
                Number = addressModel.Number,
                City = addressModel.City,
                State = addressModel.State,
                Cpf = addressModel.ClientId
            };

            context.Address.Add(address);
            context.SaveChanges();

            return context.Address.LastOrDefault().AddressId;
        }

        //Card
        public void AddCard(string session, CardModel card)
        {
            var existCard = ListCard(session).FirstOrDefault(i => i.CardNumber == card.CardNumber);
            var lines = new List<string>();

            if (existCard == null)
            {
                var creditCard = new CreditCard()
                {
                    CardNumber = card.CardNumber,
                    Name = card.Name,
                    ShelfLife = card.ShelfLife,
                    CVV = card.CVV,
                    Cpf = session
                };

                context.CreditCard.Add(creditCard);
                context.SaveChanges();
            }
        }


        //Puxa o nome do cliente
        public string ClientName(string session)
        {
            return GetEnjoyer(session).FullName;
        }

        //Verifica se o cliente possui cartão de crédito
        public bool HasCard(string session)
        {
            return ListCard(session).Count != 0;
        }

        //Traz todos os cartões cadastrados do usuário
        public List<ShowCardModel> ShowCards(string session)
        {
            return ListCard(session)
                .Select(i => new ShowCardModel
                {
                    CardNumber = i.CardNumber.ToString(),
                    Name = i.Name,
                    ShelfLife = i.ShelfLife
                })
                .ToList();
        }

        //Traz todos os endereços do usuário
        public List<ShowAddressModel> ShowAddress(string session)
        {
            var client = GetEnjoyer(session);
            var listAddress = ListAddress(session);

            return listAddress
                .Select(i => new ShowAddressModel
                {
                    ClientName = client.FullName,
                    ClientPhone = client.Phone,
                    AddressId = i.AddressId,
                    Cep = i.Cep,
                    Street = i.Street,
                    Number = i.Number,
                    City = i.City,
                    State = i.State
                })
                .ToList();
        }
    }
}
