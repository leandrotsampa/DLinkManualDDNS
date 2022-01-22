using Microsoft.OpenApi.Models;

var port = Environment.GetEnvironmentVariable("PORT");
var builder = WebApplication.CreateBuilder(args);

// Heroku pass default port by environment variable.
if (port != null)
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(Convert.ToInt32(port)));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "D-Link Manual Dynamic DNS",
        Description = "Updating Google Domains Dynamic DNS with D-Link Dynamic DNS in Manual Mode.",
        Contact = new OpenApiContact
        {
            Name = "Leandro Tavares de Melo",
            Email = "leandrotsampa@yahoo.com.br",
            Url = new Uri("https://www.linkedin.com/in/leandrotsampa/")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://github.com/leandrotsampa/DLinkManualDDNS/blob/master/LICENSE")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "D-Link Manual Dynamic DNS");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();