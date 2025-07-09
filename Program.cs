using System.Text;
using System.Text.Json;
using College2Career.Data;
using College2Career.Repository;
using College2Career.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IVacanciesRepository, VacanciesRepository>();
builder.Services.AddScoped<IApplicationsRepository, ApplicationsRepository>();
builder.Services.AddScoped<IInterviewsRepository, InterviewsRepository>();
builder.Services.AddScoped<IOffersRepository, OffersRepository>();

// Registered Services
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ICompaniesService, CompaniesService>();
builder.Services.AddScoped<ICollegesService, CollegesService>();
builder.Services.AddScoped<IStudentsService, StudentsService>();
builder.Services.AddScoped<IVacanciesService, VacanciesService>();
builder.Services.AddScoped<IApplicationsService, ApplicationsService>();
builder.Services.AddScoped<IInterviewsService, InterviewsService>();
builder.Services.AddScoped<IOffersService, OffersService>();

// Registered Helper Services
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.WebHost.UseUrls("http://10.0.2.2:7072");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Allow CORS for College2Career React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("College2Career",
        policy =>
    {
        policy.WithOrigins("https://college2career-frontend.vercel.app",
            "https://college2career-frontend.vercel.app",
                "https://college2career-frontend-git-main-aaditya-pathas-projects.vercel.app")
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

        options.Events = new JwtBearerEvents
        {
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    message = "Unauthorized! Only authorized users can access this endpoint."
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseCors("College2Career");

app.UseAuthentication();

app.UseAuthorization();

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
