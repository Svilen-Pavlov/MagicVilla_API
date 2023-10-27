
using Asp.Versioning;
using MagicVilla_Utility;
using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret"); // var to store private key

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers(options =>
{
    //options.ReturnHttpNotAcceptable = true;    // enables response of 406 codes when the type is not acceptable
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
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")));
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
    explorerOptions.SubstituteApiVersionInUrl = true; //substitutes the curly bracket formatting string with v1??? or what?
});

builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
