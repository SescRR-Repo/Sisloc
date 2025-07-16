using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configuração de cultura e fuso horário para o Brasil
var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configuração do Entity Framework para .NET 8
builder.Services.AddDbContext<SislocDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Otimizações para .NET 8
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configuração do MVC com otimizações .NET 8
builder.Services.AddControllersWithViews(options =>
{
    // Configurações específicas para MVC se necessário
})
.AddJsonOptions(options =>
{
    // Configurações para JSON serialization melhoradas no .NET 8
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Configuração de sessão com melhorias do .NET 8
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Melhoria de segurança
});

// Configuração de memória cache
builder.Services.AddMemoryCache();

// Configuração de serviços customizados
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
// builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
// builder.Services.AddScoped<IMotoristaService, MotoristaService>();

var app = builder.Build();

// Configuração do pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// Configuração das rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configuração de rotas específicas para o sistema
app.MapControllerRoute(
    name: "agendamento",
    pattern: "agendamento/{action=Index}/{id?}",
    defaults: new { controller = "Agendamento" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "veiculos",
    pattern: "veiculos/{action=Index}/{id?}",
    defaults: new { controller = "Veiculos" });

app.MapControllerRoute(
    name: "consulta",
    pattern: "consulta/{protocolo?}",
    defaults: new { controller = "Consulta", action = "Index" });

// Inicialização do banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SislocDbContext>();

    // Aplica migrations pendentes
    context.Database.Migrate();

    // Inicializa dados se necessário
    DbInitializer.Initialize(context);
}

app.Run();