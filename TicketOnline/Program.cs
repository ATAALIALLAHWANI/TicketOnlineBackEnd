using Microsoft.EntityFrameworkCore;
using TicketOnline.Models;
using TicketOnline.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<BlockedRemovalService>();
builder.Services.AddHostedService<OrderingServices>();


builder.Services.AddCors(policyBuilder =>
  policyBuilder.AddDefaultPolicy(policy =>
  policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())
);
var app = builder.Build();
app.UseCors();

//app.UseCors(policy => policy.AllowAnyHeader()
//                            .AllowAnyMethod()
//                            .SetIsOriginAllowed(origin => true)  //npm install react-scripts --save
//                            .AllowCredentials());


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