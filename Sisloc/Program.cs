using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configura��o de cultura e fuso hor�rio para o Brasil
var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configura��o do Entity Framework para .NET 8
builder.Services.AddDbContext<SislocDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Otimiza��es para .NET 8
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configura��o do MVC com otimiza��es .NET 8
builder.Services.AddControllersWithViews(options =>
{
    // Configura��es espec�ficas para MVC se necess�rio
})
.AddJsonOptions(options =>
{
    // Configura��es para JSON serialization melhoradas no .NET 8
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Configura��o de sess�o com melhorias do .NET 8
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Melhoria de seguran�a
});

// Configura��o de mem�ria cache
builder.Services.AddMemoryCache();

// Configura��o de servi�os customizados
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
// builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
// builder.Services.AddScoped<IMotoristaService, MotoristaService>();

var app = builder.Build();

// Configura��o do pipeline HTTP
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

// Configura��o das rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configura��o de rotas espec�ficas para o sistema
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

// Inicializa��o do banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SislocDbContext>();

    // Aplica migrations pendentes
    context.Database.Migrate();

    // Inicializa dados se necess�rio
    DbInitializer.Initialize(context);
}

app.Run();