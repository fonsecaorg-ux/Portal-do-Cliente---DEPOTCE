using Microsoft.EntityFrameworkCore;
using PortalCliente.Models;

namespace PortalCliente.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(AppDbContext context)
    {
        if (await context.Containers.AnyAsync())
            return;

        var hoje = DateTime.Today;

        var isotanques = new List<Container>
        {
            new() { Codigo = "DHDU1274480", Produto = "Etanol",           Cliente = "DEN HARTOGH",   Status = "Ag. Envio Estimativa", PrevisaoLiberacao = hoje.AddDays(2)  },
            new() { Codigo = "DHDL2272373", Produto = "Metanol",          Cliente = "DEN HARTOGH",   Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(5)  },
            new() { Codigo = "DHDU2273512", Produto = "Ácido acético",    Cliente = "DEN HARTOGH",   Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(3)  },
            new() { Codigo = "DHDL2413363", Produto = "Tolueno",          Cliente = "DEN HARTOGH",   Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(7)  },
            new() { Codigo = "DHDU1971099", Produto = "Etanol",           Cliente = "DEN HARTOGH",   Status = "Ag. Inspeção",         PrevisaoLiberacao = hoje.AddDays(1)  },
            new() { Codigo = "DHDU3012345", Produto = "Metanol",          Cliente = "DEN HARTOGH",   Status = "Ag. Inspeção",         PrevisaoLiberacao = hoje.AddDays(8)  },
            new() { Codigo = "DHDL3399881", Produto = "Ácido acético",    Cliente = "DEN HARTOGH",   Status = "Ag. Reparo",           PrevisaoLiberacao = hoje.AddDays(12) },
            new() { Codigo = "DHDU4455667", Produto = "Tolueno",          Cliente = "DEN HARTOGH",   Status = "Ag. Envio Estimativa", PrevisaoLiberacao = hoje.AddDays(5)  },
            new() { Codigo = "DHDL1122334", Produto = "Propilenoglicol",  Cliente = "DEN HARTOGH",   Status = "Ag. Inspeção",         PrevisaoLiberacao = hoje.AddDays(6)  },
            new() { Codigo = "DHDU3344556", Produto = "Ácido acético",    Cliente = "DEN HARTOGH",   Status = "Ag. Envio Estimativa", PrevisaoLiberacao = hoje.AddDays(4)  },
            new() { Codigo = "DHDL6677889", Produto = "Metanol",          Cliente = "DEN HARTOGH",   Status = "Ag. Reparo",           PrevisaoLiberacao = hoje.AddDays(15) },
            new() { Codigo = "DHDU8899001", Produto = "Etanol",           Cliente = "DEN HARTOGH",   Status = "Ag. Off Hire",         PrevisaoLiberacao = null             },
            new() { Codigo = "EXFU5567363", Produto = "Etanol",           Cliente = "Empresa Alpha", Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(1)  },
            new() { Codigo = "EXFU6422402", Produto = "Metanol",          Cliente = "Empresa Alpha", Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(4)  },
            new() { Codigo = "EXFU6632144", Produto = "Tolueno",          Cliente = "Empresa Alpha", Status = "Ag. Inspeção",         PrevisaoLiberacao = hoje.AddDays(14) },
            new() { Codigo = "EXFU7711223", Produto = "Hexano",           Cliente = "Empresa Alpha", Status = "Ag. Reparo",           PrevisaoLiberacao = hoje.AddDays(21) },
            new() { Codigo = "EXFU9988776", Produto = "Metanol",          Cliente = "Empresa Alpha", Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(2)  },
            new() { Codigo = "EXFU5566778", Produto = "Tolueno",          Cliente = "Empresa Alpha", Status = "Ag. Inspeção",         PrevisaoLiberacao = hoje.AddDays(10) },
            new() { Codigo = "EXFU9900112", Produto = "Tolueno",          Cliente = "Empresa Alpha", Status = "Ag. Envio Estimativa", PrevisaoLiberacao = hoje.AddDays(7)  },
            new() { Codigo = "SEDU8063278", Produto = "Ácido acético",    Cliente = "Química Beta",  Status = "Ag. Limpeza",          PrevisaoLiberacao = null             },
            new() { Codigo = "SEDU5544332", Produto = "Etanol",           Cliente = "Química Beta",  Status = "Ag. Off Hire",         PrevisaoLiberacao = null             },
            new() { Codigo = "SEDU2233445", Produto = "Etanol",           Cliente = "Química Beta",  Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(3)  },
            new() { Codigo = "SEDU7788990", Produto = "Hexano",           Cliente = "Química Beta",  Status = "Ag. Limpeza",          PrevisaoLiberacao = hoje.AddDays(1)  },
        };

        await context.Containers.AddRangeAsync(isotanques);
        await context.SaveChangesAsync();
    }
}