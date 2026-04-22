using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Library.API.Middleware.Auth;
using Library.API.Middleware.Exceptions;
using Library.Common;
using Library.Interfaces;
using Library.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Library.API.Helpers
{
    public static class StartupConfigHelper
    {
        public static void ConfigureAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthorizationSettings>(configuration.GetSection("AuthorizationSettings"));

            // add controllers
            services.AddControllers();

            ConfigureAuthServices(services, configuration);

            // DI for services and repo
            services.AddSingleton<IBookRepositoryAsync, BookRepositoryAsync>();
            services.AddScoped<IBookServiceAsync, BookServiceAsync>();

            services.AddSingleton<IUserService, UserService>();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            ConfigureSwagger(services);
        }

        public static void ConfigureApp(this WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();
            app.UseRouting();

            UseAuth(app);

            UseSwagger(app);

            app.MapControllers();
        }

        private static void ConfigureAuthServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["AuthorizationSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["AuthorizationSettings:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthorizationSettings:Secret"] ?? string.Empty)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization(config => {
                config.AddPolicy(Policies.User, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.User)));
                config.AddPolicy(Policies.Admin, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.Admin)));
                config.AddPolicy(Policies.All, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.All)));
            });

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        private static void UseAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AttachUserToContextMiddleware>();
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {           
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Library API",
                    Description = "A simple book management ASP.NET Core Web API"
                });

                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "Library API with Auth",
                    Description = "A simple book management ASP.NET Core Web API with Auth"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter only the JWT token"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        private static void UseSwagger(IApplicationBuilder app)
        {

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Library V2");

                c.RoutePrefix = string.Empty;
            });


        }
    }
}
