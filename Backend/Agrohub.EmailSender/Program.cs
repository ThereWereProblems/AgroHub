using Agrohub.Common.Extensions;
using Agrohub.EmailSender.Options;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var Smtp = builder.Configuration.GetSection("Smtp");
builder.Services.Configure<MailOptions>(Smtp);

builder.Services.AddMediatR(config =>config.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
builder.Services.AddMessageBroker(builder.Configuration, Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();
