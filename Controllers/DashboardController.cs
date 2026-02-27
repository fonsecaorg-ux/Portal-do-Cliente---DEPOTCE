DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using PortalCliente.Models;

namespace PortalCliente.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var lista = new List<Isotank>
            {
                new Isotank { Codigo = "ISO123", Produto = "Xileno", Status = "Em Inspeção", PrevisaoLiberacao = DateTime.Now.AddDays(2) },
                new Isotank { Codigo = "ISO124", Produto = "Etanol", Status = "Disponível", PrevisaoLiberacao = null },
                new Isotank { Codigo = "ISO125", Produto = "Metanol", Status = "Em Descarga", PrevisaoLiberacao = DateTime.Now.AddDays(1) }
            };

            return View(lista);
        }
    }
}