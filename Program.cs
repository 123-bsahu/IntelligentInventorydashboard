using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Features.ApproveVehicleUpdate;
using IntelligentInventoryDashboard.Features.CreateVehicle;
using IntelligentInventoryDashboard.Features.GetInventory;
using IntelligentInventoryDashboard.Features.LogAgingAction;
using IntelligentInventoryDashboard.Features.RequestVehicleUpdate;
using IntelligentInventoryDashboard.Features.SearchVehicles;
using IntelligentInventoryDashboard.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Feature handlers (vertical slices)
builder.Services.AddScoped<GetInventoryHandler>();
builder.Services.AddScoped<CreateVehicleHandler>();
builder.Services.AddScoped<RequestUpdateHandler>();
builder.Services.AddScoped<ApproveUpdateHandler>();
builder.Services.AddScoped<LogAgingActionHandler>();
builder.Services.AddScoped<SearchVehiclesHandler>();
builder.Services.AddHttpClient<NLPSearchService>();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Authentication: JWT Bearer, so [Authorize(Roles = "SeniorManager")] works.
//var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IntelligentInventoryDashboard";
//var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IntelligentInventoryDashboard";
//var jwtKey = builder.Configuration["Jwt:Key"]
//    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = jwtIssuer,
//            ValidAudience = jwtAudience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//        };
//    });

//builder.Services.AddAuthorization();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles(); // serves wwwroot/index.html at the site root
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // Dev-only helper: mints a JWT with the given role for local testing, e.g.
    // POST /dev/token?role=SeniorManager
    //    app.MapPost("/dev/token", (string role) =>
    //    {
    //        var claims = new List<Claim> { new(ClaimTypes.Role, role), new(ClaimTypes.Name, $"dev-{role.ToLowerInvariant()}") };
    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    //        var token = new JwtSecurityToken(
    //            issuer: jwtIssuer,
    //            audience: jwtAudience,
    //            claims: claims,
    //            expires: DateTime.UtcNow.AddHours(8),
    //            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

    //        return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    //    });
    //}

    app.MapControllers();



    app.Run();
}