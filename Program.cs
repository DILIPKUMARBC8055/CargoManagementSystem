using CargoManagement.Services;
using CargoManagementProject.core.Interfaces;
using CargoManagementProject.core.Services;
using CargoManagementProject.Core.Entities;
using CargoManagementProject.Infrastructure.Data;
using CargoManagementProject.Infrastructure.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CargoManagementProject.Infrastructure.Logging;
using CargoManagementProject.Infrastructure.Repositories;
using CargoManagement.Core.Services;
using CargoManagement.Api.Services;
using CargoManagement.Core.Interfaces;
using CargoManagement.Infrastructure.Repositories;

using NLog.Web;
using NLog;




// Add services to the container.

//Nloger 
var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
try
{

    logger.Debug("init main");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();








    //  Adding DbContext with SQL Server
    builder.Services.AddDbContext<CargoDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    //configuring redis 
    var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfiguration;
        options.InstanceName = "CargoProjectRedis";

    });
    builder.Services.AddScoped<ICacheService, CacheService>();

    builder.Services.AddScoped<ICargoRepository, CargoRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Register other repositories similarly
    builder.Services.AddScoped<ICargoService, CargoService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    //Registering logger
    builder.Services.AddSingleton<CargoManagementProject.Infrastructure.Logging.Logger>();

    //Registering jwt token
    builder.Services.AddSingleton<JwtTokenService>();

    //jwt tocken is configured
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // Should be true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
    //adding controllers
    builder.Services.AddControllers();

    //swagger implementation to have jwt tocken 
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                          "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                          "Example: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
        });
    });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            policy =>
            {
                policy.WithOrigins("http://localhost:44347") // Replace with your frontend's origin
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Custom Logging Middleware (Optional)
    app.UseMiddleware<RequestLoggingMiddleware>();


    app.UseHttpsRedirection();

    //  Use Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Controllers
    app.MapControllers();

    // 2.6. Initialize Database (Optional)
    // Ensure the database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CargoDbContext>();
        dbContext.Database.Migrate();
    }




    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}


