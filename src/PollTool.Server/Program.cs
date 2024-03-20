using Microsoft.AspNetCore.Cors.Infrastructure;
using PollTool.Server.Models.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PollContext>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Polltool.Session";
    options.IdleTimeout = TimeSpan.FromDays(60);
    options.Cookie.IsEssential = true;
});


builder.Logging.ClearProviders(); 
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();