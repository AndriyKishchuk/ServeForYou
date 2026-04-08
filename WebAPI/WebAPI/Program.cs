using System.Text;
using DataBase.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using WebAPI.Factory;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IEntityFactory, EntityFactory>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();


builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 20 * 1024 * 1024);


var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AplicationContext>();
    db.Database.Migrate();

    var defaultCompany = db.Companies.OrderBy(c => c.Id).FirstOrDefault();
    if (defaultCompany is null)
    {
        db.Companies.Add(new Company { CompanyName = "ServeForYou" });
        db.SaveChanges();
    }
    else if (defaultCompany.CompanyName != "ServeForYou")
    {
        defaultCompany.CompanyName = "ServeForYou";
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowReact");
app.UseHttpsRedirection();
app.UseStaticFiles();         
app.UseAuthentication();  
app.UseAuthorization();
app.MapControllers();
app.Run();

