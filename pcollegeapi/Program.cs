using System.IO.Compression;
using Flyurdreamapi.Extensions;
using Flyurdreamapi.RoleMiddleware;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

IConfiguration configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json").AddUserSecrets<Program>().AddEnvironmentVariables().Build();
ConfigurationData.DbConnectionString = configuration.GetConnectionString("fudSQLConnection");
ConfigurationData.BlobConnectionString = configuration.GetConnectionString("AzureStorageAccount");
ConfigurationData.BlobContainerName = configuration.GetConnectionString("ContainerName");
ConfigurationData.BlobRootURI = configuration.GetConnectionString("blobrooturi");
string WhitelistClientUrl = configuration["WhitelistClientUrl"];
string Headers = configuration["Headers"];
//var jwtSettings = new JwtSettings();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    //.WriteTo.console(new CompactJsonFormatter())
    .CreateLogger();
// Add services to the container.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(f => Log.Logger);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Enable for HTTPS requests
    options.Providers.Add<GzipCompressionProvider>(); // Add Gzip compression
    options.Providers.Add<BrotliCompressionProvider>(); // Add Brotli compression
});

// Configure response compression options
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest; // Set Brotli compression level
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal; // Set Gzip compression level
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServices(configuration);
builder.Services.Configure<CachingSettings>(configuration.GetSection("Caching"));
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors",
        builder =>
        {
            builder.WithOrigins(WhitelistClientUrl)
                 .WithHeaders(Headers)
                  .AllowAnyMethod()
                   .AllowCredentials();
        });

});
var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseCors("cors");
app.UseResponseCompression();
app.UseSwagger();
//app.UseSwaggerUI();
app.UseSwaggerUI(options =>
{
    options.DefaultModelsExpandDepth(-1);
});
app.UseHttpsRedirection();

app.UseAuthorization();
//app.UseRoleMiddleware();
app.MapControllers();

app.Run();
