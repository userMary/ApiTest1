using System.Reflection;
using System.Text;
using ApiTest1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// получаем строку подключения из файла конфигурации
string connection = builder.Configuration.GetConnectionString("DefaultConnection")!; // или string?

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // CookieAuthenticationDefaults.AuthenticationScheme - прописать схему, если нужны куки, а не JWT
    .AddCookie()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your_issuer",
            ValidAudience = "your_audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("rOe7o1N80fT1+f4tw5o4oxHRVGcoiYZ1ow5hIBEHvtk=")),
            ClockSkew = TimeSpan.FromMinutes(1) // Погрешность времени
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Admin"));
});

// Add Cors (b - это builder)
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", b =>
{
    string allowOrigins = builder.Configuration.GetValue<string>("AllowOrigins");
    b.WithOrigins(allowOrigins) // WithOrigins("http://localhost:4200")
           .WithMethods("POST", "GET")
           .AllowCredentials() // WithMethods("POST", "GET")
           .AllowAnyHeader();
}));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(options =>
//{
//    // using System.Reflection;
//    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
//});
builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("v1", new OpenApiInfo
    //{
    //    Version = "v1",
    //    Title = "ToDo API My API",
    //    Description = "An ASP.NET Core Web API for managing ToDo items",
    //    TermsOfService = new Uri("https://example.com/terms"),
    //    Contact = new OpenApiContact
    //    {
    //        Name = "Example Contact",
    //        Url = new Uri("https://example.com/contact")
    //    },
    //    License = new OpenApiLicense
    //    {
    //        Name = "Example License",
    //        Url = new Uri("https://example.com/license")
    //    }
    //});

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); //обработчик если есть ошибка, развернутый фитбэек - текст ошибки
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseAuthentication();
app.UseAuthorization();

// устанавливаем сопоставление маршрутов с контроллерами
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
