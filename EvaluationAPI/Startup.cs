using EvaluationAPI.Models;
using EvaluationAPI.Repository;
using EvaluationAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace EvaluationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "EvaluationAPI",
                    Description = "EvaluationAPI",
                    Version = "v1"
                });

                var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                option.IncludeXmlComments(filePath);
            });
            services.AddCors(options => options.AddPolicy("AuthPolicy", builder =>
            {
                builder
                    .WithOrigins(Constants.AUTH_ALLOWED_ORIGINS)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .WithMethods("POST");
            }));

            services.AddCors(options => options.AddPolicy("EvaluationRulePolicy", builder =>
            {
                builder
                    .WithOrigins(Constants.EVALUATION_RULE_ALLOWED_ORIGINS)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));

            services.AddCors(options => options.AddPolicy("EvaluationPolicy", builder =>
            {
                builder
                    .WithOrigins(Constants.EVALUATION_ALLOWED_ORIGINS)
                    .AllowAnyHeader()
                    .WithMethods("POST");
            }));
 
            services
                .AddDbContext<AuthRepository>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr"), builder =>
                {
                    builder.EnableRetryOnFailure(1, TimeSpan.FromSeconds(1), null);
                }))
                .AddIdentity<EvaluationUser, IdentityRole>()
                .AddEntityFrameworkStores<AuthRepository>()
                .AddDefaultTokenProviders();

            services
                .AddDbContext<EvaluationRulesRepository>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr"), builder =>
                {
                    builder.EnableRetryOnFailure(1, TimeSpan.FromSeconds(1), null);
                }));

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["JWT:ValidIssuer"],
                        ValidateAudience = true,
                        ValidAudience = Configuration["JWT:ValidAudience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                    };
                });

            services
                .AddScoped<EvaluationRuleService>()
                .AddScoped<EvaluationService>()
                .AddControllers();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EvaluationAPI");
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
