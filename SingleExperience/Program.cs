using SingleExperience.Views;
using SingleExperience.Services.CartServices;
using SingleExperience.Entities.DB;
using SingleExperience.Services.CartServices.Models;

namespace SingleExperience
{
    class Program
    {
        private static Context.SingleExperience context;

        static void Main(string[] args)
        {
            SingleExperience.Context.SingleExperience context = new SingleExperience.Context.SingleExperience();
            //Carrinho memória
            CartService cartService = new CartService(context);
            SessionModel parameters = new SessionModel();
            CartModel model = new CartModel();
            parameters.CartMemory = cartService.AddItensMemory(model, parameters.CartMemory);            

            //Chama a função para pegar o IP do PC
            ClientDB client = new ClientDB();
            parameters.Session = client.GetIP();

            //Chama a função para pegar a quantidade que está no carrinho
            var countProducts = cartService.TotalCart(parameters);
            parameters.CountProduct = countProducts.TotalAmount;

            //Chama a home para ser exibida inicialmente
            ClientHomeView inicio = new ClientHomeView();
            inicio.ListProducts(parameters);
        }
    }
}
