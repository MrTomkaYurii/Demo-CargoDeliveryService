using Drivers.Api.Helpers;
using Drivers.Api.Middleware;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Validation;
using Drivers.BLL.Managers;
using Drivers.BLL.Managers.Contracts;
using Drivers.DAL.ADO.Repositories;
using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Helpers;
using Drivers.DAL.EF.Repositories;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW;
using Drivers.DAL.EF.UOW.Contracts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SixLabors.ImageSharp;
using System.Data;
using System.Text;



//КОНФІГРУВАННЯ: 1) файли конфігурацій 2) IOC 3) логіювання
var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////////////////////////////////////////////////////
/// ADO.NET = Dapper
////////////////////////////////////////////////////////////////////////////////////

// Connection/Transaction for ADO.NET/DAPPER database
builder.Services.AddScoped((s) => new SqlConnection(builder.Configuration.GetConnectionString("MSSQLConnection")));
    builder.Services.AddScoped<IDbTransaction>(s =>
    {
        SqlConnection conn = s.GetRequiredService<SqlConnection>();
        conn.Open();
        return conn.BeginTransaction();
    });

// Dependendency Injection for Repositories/UOW from ADO.NET DAL
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<ITruckRepository, TruckRepository>();
builder.Services.AddScoped<IRepairRepository, RepairRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

////////////////////////////////////////////////////////////////////////////////////
/// Enity Framework
////////////////////////////////////////////////////////////////////////////////////
//Connection for EF database + DbContext
builder.Services.AddDbContext<DriversManagementContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
    options.UseSqlServer(connectionString);
});

// Dependendency Injection for Repositories/UOW from EF DAL
builder.Services.AddScoped<IEFDriverRepository, EFDriverRepository>();
builder.Services.AddScoped<IEFCompanyRepository, EFCompanyRepository>();
builder.Services.AddScoped<IEFExpenseRepository, EFExpenseRepository>();
builder.Services.AddScoped<IEFInspectionRepository, EFInspectionRepository>();
builder.Services.AddScoped<IEFPhotoRepository, EFPhotoRepository>();
builder.Services.AddScoped<IEFRepairRepository, EFRepairRepository>();
builder.Services.AddScoped<IEFTruckRepository, EFTruckRepository>();
builder.Services.AddScoped<IEFUnitOfWork, EFUnitOfWork>();

// Dependendency Injection for Helpers
builder.Services.AddScoped<ISortHelper<EFDriver>, SortHelper<EFDriver>>();

// Dependendency Injection for Managers
builder.Services.AddScoped<IDriversManager, DriversManager>();
builder.Services.AddScoped<ICompanyManager, CompanyManager>();
builder.Services.AddScoped<IExpenseManager, ExpenseManager>();
builder.Services.AddScoped<IInspectationManager, InspectationManager>();
builder.Services.AddScoped<IPhotoManager, PhotoManager>();
builder.Services.AddScoped<IRepairManager, RepairManager>();
builder.Services.AddScoped<ITruckManager, TruckManager>();

////////////////////////////////////////////////////////////////////////////////////
/// APPLICATION
////////////////////////////////////////////////////////////////////////////////////
//MIDLLEWARE
builder.Services.AddTransient<ExceptionMiddleware>();
//AUTOMAPPER
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(Drivers.BLL.Mapping.BLLMappingProfile).Assembly);
//FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<IValidator<MiniDriverReqDTO>, MiniDriverReqDTO_Validator>();
builder.Services.AddScoped<IValidator<Drivers.DAL.EF.Entities.HelpModels.DriverParameters>, DriverParametersValidator>();
builder.Services.AddScoped<IValidator<DateRange>, DateRangeValidator>();
builder.Services.AddScoped<IValidator<CompanyReqDTO>, CompanyReqDTO_Validator>();
builder.Services.AddScoped<IValidator<ExpenseReqDTO>, ExpenseReqDTO_Validator>();
builder.Services.AddScoped<IValidator<InspectionReqDTO>, InspectionReqDTO_Validator>();
builder.Services.AddScoped<IValidator<RepairReqDTO>, RepairReqDTO_Validator>();

//JWT TOKEN VALIDATION FOR => AuthJWTModule
//var jwt = builder.Configuration.GetSection("JWT").Get<jwtConfig>();
//builder.Services.AddAuthentication(opt =>
//{
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,

//            ValidIssuer = jwt.ValidIssuer,
//            ValidAudience = jwt.ValidAudience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwt.Secret))
//        };
//    });

//JWT TOKEN VALIDATION FOR => DuendeIdentityInMemory
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration.GetSection("Auth:Authority").Get<string>();
        options.Audience = "DriverApi";
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
    });

// Controllers
builder.Services.AddControllers();

//Swagger / OpenAPI => AuthorizationModuleAPI(access & refresh token)
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(opt =>
//{
//    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "DriverAPI", Version = "v1" });
//    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Please enter token",
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        BearerFormat = "JWT",
//        Scheme = "bearer"
//    });
//    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type=ReferenceType.SecurityScheme,
//                    Id="Bearer"
//                }
//            },
//            new string[]{}
//        }
//    });
//});

// Swagger/OpenAPI => Duende Identity server (Authorization Code Grant)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "DriverAPI", Version = "v1" });

    // Додаємо Security Definition для OAuth 2.0 с Authorization Code Grant
    opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration.GetSection("Auth:Swagger:AuthorizationUrl").Get<string>()),
                TokenUrl = new Uri(builder.Configuration.GetSection("Auth:Swagger:TokenUrl").Get<string>()),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID scope" },
                    { "profile", "Profile scope" },
                    { "DriverApi.read", "Read access to Driver API" },
                    { "DriverApi.write", "Write access to Driver API" }
                }
            }
        }
    });

    // Додаємо Security Requirement для вказівки що використовується OAuth 2.0
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile", "DriverApi.read", "DriverApi.write" }
        }
    });
});

////////////////////////////////////////////////////////////////////////////////////////////////////////
///PIPELINE
////////////////////////////////////////////////////////////////////////////////////////////////////////

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Access+Refresh token
    //app.UseSwagger();
    //app.UseSwaggerUI();

    // Autorization Code Flow & PKCE:
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthUsePkce();
    });
}
// Global cors policy
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.Run();