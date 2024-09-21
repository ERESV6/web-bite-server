using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Hubs;
using web_bite_server.Models;
using web_bite_server.Repository;
using web_bite_server.Services.CardGame;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSignalRSwaggerGen();
});

builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebBiteConnectionString"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultChallengeScheme =
    o.DefaultSignInScheme =
    o.DefaultSignOutScheme =
    CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<CardGameConnectionRepository>();
builder.Services.AddScoped<CardGameCardRepository>();
builder.Services.AddScoped<CardGameGameRepository>();

builder.Services.AddScoped<CardGameConnectionService>();
builder.Services.AddScoped<CardGameCardService>();
builder.Services.AddScoped<CardGameGameService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CardGameHub>("card-game-hub");

app.UseExceptionHandler();

app.Run();


/**
    REFACTOR:
    - minmax card to constants
    - userConnection.HitPoints to constant, jakis gameconfig ogarnac
    - rozdzial controler service repository na routes i wszystko per katalog
    - mappery

*/

