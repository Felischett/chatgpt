using Microsoft.AspNetCore.Identity;
using ORM.Services;
using Models;
using WebApi_Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbManager>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// DataProtection + Crypto-Service
builder.Services.AddDataProtection();
builder.Services.AddSingleton<MessageCryptoService>();
builder.Services.AddHttpClient<DeeplTranslationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
