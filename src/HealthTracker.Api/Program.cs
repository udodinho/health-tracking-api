using System.Text;
using HealthTracker.Authentication.Configuration;
using HealthTracker.DataService.Data;
using HealthTracker.DataService.IConfiguration;
using HealtTracker.DataService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Update the JWT config from the settings
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Get Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext for the API
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options => 
options.UseNpgsql(connectionString));
// builder.Services.AddDbContext<AppDbContext>(options => 
// options.UseSqlServer(connectionString));

// Add dependency injection for UnitOfWork
IServiceCollection serviceCollection = builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Getting the secret from the config
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,
    ValidateAudience = false,
    RequireExpirationTime = false,
    ValidateLifetime = true,
};

// Injecting into our DI container
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt => {
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddIdentityCore<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true )
.AddEntityFrameworkStores<AppDbContext>();
    


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthTracker.Api ", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

    
});

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
 {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Health Tracker API v1");
    c.RoutePrefix = "";
 });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
