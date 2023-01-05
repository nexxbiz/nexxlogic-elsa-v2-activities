using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Extensions;

var builder = WebApplication.CreateBuilder(args);

var elsaSection = builder.Configuration.GetSection("Elsa");
builder.Services.AddElsa(options =>
{
    options
        .AddConsoleActivities()
        .AddJavaScriptActivities()
        .AddHttpActivities(elsaSection.GetSection("Server").Bind)
        .AddSoapConnector()
        .AddSoapActivitiesProvider(builder.Configuration.GetSection("SoapActivitySettings").Bind);

    options.UseEntityFrameworkPersistence(ef =>
    {
        ef.UseSqlite();
    });
});

builder.Services.AddElsaApiEndpoints()
    .AddElsaSwagger();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app
    .UseStaticFiles()
    .UseCors()
    .UseHttpActivities()
    .UseRouting()
    .UseEndpoints(endpoints =>
    {
        // Elsa API Endpoints are implemented as regular ASP.NET Core API controllers.
        endpoints.MapControllers();
        endpoints.MapFallbackToPage("/_Host");
    });

app.Run();