using ComputerRepairs;
using ComputerRepairs.Data;
using ComputerRepairs.Middleware;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Computer Repairs Application",
        Version = "v1"
    });
    var security = new OpenApiSecurityScheme
    {
        Name = HeaderNames.Authorization, // Header name
        Type = SecuritySchemeType.ApiKey, // Security type
        In = ParameterLocation.Header, // Provided token location
        Description = "JWT Authorization header", // Decription for UI
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme,
        }
    };
    x.AddSecurityDefinition(security.Reference.Id, security); // Adds the security definition to the OpenAPI document
    x.AddSecurityRequirement(new OpenApiSecurityRequirement { { security, Array.Empty<string>()} });
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddSingleton<JwtOptions>();

var allowedOrigin = "Frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigin, policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowCredentials().AllowAnyHeader();
    });
});

var config = builder.Configuration;

var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDBContext>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwtOptions = config.GetSection("Jwt").Get<JwtOptions>();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme =
    x.DefaultChallengeScheme =
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.SigningKey ?? throw new InvalidOperationException("JWT Key is missing in config file"))),
        ValidIssuer = jwtOptions?.Issuer,
        ValidAudience = jwtOptions?.Audience,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors(allowedOrigin);


app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["access"];
    if (string.IsNullOrEmpty(token))
    {
        Log.Warning("User made the below request with no access token");
    }
    else
    {
        context.Request.Headers.Append("Authorization", "Bearer " + token);
    }
    await next(context);
});
app.UseMiddleware<TimeLoggerMiddleware>();
//app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = ["Employee", "Admin"];

    foreach (string role in roles)
    {
        if(!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();
