using SingleExperience.Views;
using SingleExperience.Services.CartServices;
using SingleExperience.Services.CartServices.Models;
using SingleExperience.Services.ClientServices;

namespace SingleExperience
{
    class Program
    {
        static void Main(string[] args)
        {
            SingleExperience.Context.SingleExperience context = new SingleExperience.Context.SingleExperience();
            //Carrinho memória
            CartService cartService = new CartService(context);
            SessionModel parameters = new SessionModel();
            CartModel model = new CartModel();
            parameters.CartMemory = cartService.AddItensMemory(model, parameters.CartMemory);            

            //Chama a função para pegar o IP do PC
            ClientService clientService = new ClientService(context);
            parameters.Session = clientService.GetIP();

            //Chama a função para pegar a quantidade que está no carrinho
            var countProducts = cartService.Total(parameters);
            parameters.CountProduct = countProducts.TotalAmount;

            //Chama a home para ser exibida inicialmente
            ClientHomeView inicio = new ClientHomeView();
            inicio.ListProducts(parameters);
        }
    }
}
