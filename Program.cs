using System.Text;
using College2Career.Data;
using College2Career.Repository;
using College2Career.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Added connection string to the configuration
//var configuration = builder.Configuration;
//Console.WriteLine("JWT Key: " + configuration["Jwt:Key"]);

// Register DbContext with SQL Server
builder.Services.AddDbContext<C2CDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("College2CareerConnectionString"));
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7072);
});


// Registered Repository
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddScoped<ICollegesRepository, CollegesRepository>();
builder.Services.AddScoped<IStudentsRepository, StudentsRepository>();


// Registered Services
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ICompaniesService, CompaniesService>();
builder.Services.AddScoped<ICollegesService, CollegesService>();
builder.Services.AddScoped<IStudentsService, StudentsService>();


// Registered Helper Services
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://0.0.0.0:7072");

// Allow CORS for College2Career React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("College2Career",
        policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// JWT Authentication Configuration
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("College2Career");

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
