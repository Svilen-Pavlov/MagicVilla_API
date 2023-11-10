
using Asp.Versioning;
using MagicVilla_Utility;
using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var JWTprivateKey = builder.Configuration.GetValue<string>("ApiSettings:Secret"); // var to store private key
var connString = builder.Configuration.GetConnectionString("DefaultSQLConnection");

builder.Services
    .AddAuthentication(authOptions =>
{
    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme; // remove
})
    .AddJwtBearer(bearerOpt =>
{
    bearerOpt.RequireHttpsMetadata = false;
    bearerOpt.SaveToken = true;
    bearerOpt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JWTprivateKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers(options =>
{
    //options.ReturnHttpNotAcceptable = true;    // enables response of 406 codes when the type is not acceptable
    options.CacheProfiles.Add("Default30", new CacheProfile()
    {
        Duration = 30
    });
})
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(StaticDetails.JWTAuthenticationHeaderName, new OpenApiSecurityScheme
    {
        Description = //a user explanation on how to use the below 3 configs
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization", // the API request header key name to be used
        In = ParameterLocation.Header, //configures the location of the parameters name and scheme. In this case the request Header
        Scheme = StaticDetails.JWTAuthenticationHeaderName // "Bearer". the API request header value name to be used before pasting the token string
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = StaticDetails.JWTAuthenticationHeaderName
                            },
                Scheme = "oauth2",
                Name = StaticDetails.JWTAuthenticationHeaderName,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Version = "v1.0",
        Title = "Magic Villa V1",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://example.com.terms"),
        Contact = new OpenApiContact()
        {
            Name = "Svilen",
            Url = new Uri("https://github.com/Svilen-Pavlov/")
        },
        License = new OpenApiLicense()
        {
            Name = "Example License",
            Url = new Uri("https://exapmle.com/license")
        }
    });
    options.SwaggerDoc("v2", new OpenApiInfo()
    {
        Version = "v2.0",
        Title = "Magic Villa V2",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://example.com.terms"),
        Contact = new OpenApiContact()
        {
            Name = "Svilen Pavlov",
            Url = new Uri("https://github.com/Svilen-Pavlov/")
        },
        License = new OpenApiLicense()
        {
            Name = "Example License",
            Url = new Uri("https://exapmle.com/license")
        }
    });
});

builder.Services.AddDbContext<ApplicationDBContext>(option =>
    option.UseSqlServer(connString, //configures connection string
    sqlServerOptionsAction: builder =>
    {
        builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); //fixes transient resilliency errors when connecting to Azure DB Server
    }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.ConfigureApplicationCookie(options => //returns 401 instead of 404 for unauthorized requests
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Headers["Location"] = context.RedirectUri;
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services
    .AddApiVersioning(versioningOptions =>
{
    versioningOptions.ReportApiVersions = true;
    versioningOptions.DefaultApiVersion = new ApiVersion(1, 0);
    versioningOptions.AssumeDefaultVersionWhenUnspecified = true;

})
    .AddApiExplorer(explorerOptions =>
{
    explorerOptions.GroupNameFormat = "'v'VVV"; //formats version as "'v'major[.minor][-status]"
    explorerOptions.SubstituteApiVersionInUrl = true; //substitutes the curly bracket formatting string with the version selected in the Select a Definition top Swagger field
});

builder.Services.AddResponseCaching();

builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");
});

if (app.Environment.IsDevelopment())
{

}
else
{
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty; //swagger won't run when deployed without this. Test by publishing
    });
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
