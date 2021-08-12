using SingleExperience.Entities;
using SingleExperience.Services.ClientServices.Models;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SingleExperience.Services.UserServices
{
    public class UserService
    {
        protected readonly SingleExperience.Context.SingleExperience context;

        public UserService(SingleExperience.Context.SingleExperience context)
        {
            this.context = context;
        }


        public string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string session = "";

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    session = ip.ToString().Replace(".", "");
                }
            }
            return session;
        }

        public User SignIn(SignInModel signIn)
        {
            var client = GetEnjoyer(signIn.Email);
            User session = null;

            if (client != null)
            {
                if (client.Password == signIn.Password)
                {
                    session = client;
                }
            }

            return session;
        }

        //Sair
        public string SignOut()
        {
            return GetIP();
        }

        public User GetEnjoyer(string cpf)
        {
            return context.Enjoyer
                .FirstOrDefault(i => i.Cpf == cpf || i.Email == cpf);
        }

        public void SignUp(SignUpModel enjoyer)
        {
            var user = new User()
            {
                Cpf = enjoyer.Cpf,
                FullName = enjoyer.FullName,
                Phone = enjoyer.Phone,
                Email = enjoyer.Email,
                BirthDate = enjoyer.BirthDate,
                Password = enjoyer.Password,
                Employee = enjoyer.Employee
            };

            context.Enjoyer.Add(user);
            context.SaveChanges();
        }
    }
}
