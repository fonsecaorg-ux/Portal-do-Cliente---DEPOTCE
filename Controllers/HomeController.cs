using Microsoft.AspNetCore.Mvc;
using PortalCliente.Models;

namespace PortalCliente.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var lista = new List<Isotank>
            {
                new Isotank
                {
                    Codigo = "ISO123",
                    Produto = "Metanol",
                    Status = "Em trânsito",
                    PrevisaoLiberacao = DateTime.Now.AddDays(5)
                },
                new Isotank
                {
                    Codigo = "ISO456",
                    Produto = "Etanol",
                    Status = "No porto",
                    PrevisaoLiberacao = DateTime.Now.AddDays(2)
                }
            };

            return View(lista);
        }
    }
}