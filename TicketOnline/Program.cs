using Microsoft.EntityFrameworkCore;
using TicketOnline.Models;
using TicketOnline.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//builder.Services.AddHostedService<BlockedRemovalService>();
//builder.Services.AddHostedService<OrderingServices>();
//builder.Services.AddHostedService<Notification2>();
string url = Networking.GenerateUrl();
//if (!string.IsNullOrEmpty(url))
//{
//builder.WebHost.UseUrls(url);
////}
//Console.WriteLine(url);

//builder.Services.AddCors(policyBuilder =>
//  policyBuilder.AddDefaultPolicy(policy =>
//  policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())// npm install react-scripts --save
//);


var app = builder.Build();

app.UseCors(policy => policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(origin => true)
                            .AllowCredentials());

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