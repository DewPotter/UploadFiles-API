using Microsoft.EntityFrameworkCore;
using UploadFiles.Data;
using UploadFiles.Interfaces;
using UploadFiles.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TransactionDataContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DatabaseSettings") ??
        throw new InvalidOperationException("Connection string not found"))
);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITransactionDataService, TransactionDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.    
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
