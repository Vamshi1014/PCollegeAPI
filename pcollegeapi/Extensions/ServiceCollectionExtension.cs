using System.Text;
using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Flyurdreamcommands.Service.Abstract;
using Flyurdreamcommands.Service.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Flyurdreamapi.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IMenuRepository, MenuRepository>();
            services.AddTransient<ICommonRepository, CommonRepository>();
            services.AddTransient<IEnquiryRepository, EnquiryRepository>();
            services.AddTransient<IPartnerRepository, PartnerRepository>();
            services.AddTransient<IBlobHandler, BlobHandler>();
            services.AddScoped<ExcelToDataTable>();
            services.AddScoped<IDataMapping, DataMapping>();
            services.AddScoped<IQuestionsRepository, QuestionsRepository>();
            services.AddTransient<IDocumentRepository, DocumentRepository>();
            services.AddTransient<IReferenceRepository, ReferenceRepository>();
            services.AddTransient<IInsertMasterData, InsertMasterData>();
            services.AddTransient<IAddressRepository, AddressRepository>();
            services.AddTransient<IVoipRepository, VoipRepository>();       
            services.AddTransient<IProgramRepository,ProgramRepository > ();
            services.AddTransient<IStudentRepository,StudentRepository>();
            services.AddTransient<IUniversityRepository, UniversityRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddTransient<IAgentRepository, AgentRepository>();


            // Register AuthorizeAction with its dependencies
            //   services.AddScoped<AuthorizeAction>();

            services.AddHttpClient();
            services.AddLogging();

            // Accessing the configuration for JwtToken settings
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                        };
                    });

            return services;
        }
    }

}
