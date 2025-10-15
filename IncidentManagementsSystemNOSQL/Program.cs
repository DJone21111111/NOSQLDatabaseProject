using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using IncidentManagementsSystemNOSQL.Service;              // Interfaces + TicketPriorityService
using IncidentManagementsSystemNOSQL.Service.IncidentManagementsSystemNOSQL.Service;
using Microsoft.Extensions.Options;
using MongoDB.Bson;                                         // optional (for ping)
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Mongo client & DB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cs = builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString");
    return new MongoClient(cs);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// App services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddScoped<ITicketService, TicketService>();        // once
builder.Services.AddScoped<ITicketPriorityService, TicketPriorityService>();

builder.Services.AddScoped<IUserService, UserService>();            // ensure UserService : IUserService

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Optional: verify Mongo + ensure indexes on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    db.RunCommand<BsonDocument>(new BsonDocument("ping", 1)); // throws fast if bad conn string
    scope.ServiceProvider.GetRequiredService<ITicketRepository>().EnsureIndexes();
}

app.Run();
