using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalCliente.Data;
using PortalCliente.Models;
using PortalCliente.Services;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=portalcliente.db"));

// Identity com roles
builder.Services.AddIdentity<UsuarioAplicacao, IdentityRole>(options =>
{
    // Regras de senha — ajuste conforme necessário
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Origem dos dados (MBM ou local)
var mbmBaseUrl = builder.Configuration["MBM:BaseUrl"]?.Trim();
if (!string.IsNullOrWhiteSpace(mbmBaseUrl))
{
    builder.Services.AddHttpClient<MbmOrigenDadosService>(client =>
    {
        client.BaseAddress = new Uri(mbmBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    });
    builder.Services.AddScoped<IOrigenDadosService, MbmOrigenDadosService>();
}
else
{
    builder.Services.AddScoped<IOrigenDadosService, LocalOrigenDadosService>();
}

builder.Services.AddScoped<IConfiguracaoContatoService, ConfiguracaoContatoService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<PortalCliente.Filters.ObrigarTrocaSenhaFilter>();
});

var app = builder.Build();

// Aplicar migrations e seed de dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Aplica migrations pendentes automaticamente
    db.Database.Migrate();

    // Garante que a tabela ObservacoesIsotank existe (caso a migration não tenha sido aplicada antes)
    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ObservacoesIsotank (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CodigoIsotank TEXT NOT NULL,
            Texto TEXT NOT NULL,
            DataHora TEXT NOT NULL,
            AutorNome TEXT NOT NULL
        );");

    // Seed de isotanques (banco local)
    await SeedData.EnsureSeedAsync(db);

    // Seed de roles e usuário admin inicial
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioAplicacao>>();
    await IdentitySeedData.EnsureSeedAsync(roleManager, userManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication(); // deve vir antes de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();