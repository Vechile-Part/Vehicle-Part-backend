using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehiclePart.Application.Interfaces;
using VehiclePart.Infrastructure;
using VehiclePart.Infrastructure.Data;
using Vehicle_Part.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSecret = builder.Configuration["JWT:Secret"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT:Secret must be configured and at least 32 characters long.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database");

    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        log.LogError(ex, "EF Core MigrateAsync failed; continuing with schema repair.");
    }

    await db.Database.ExecuteSqlRawAsync(
        """ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ProfilePictureUrl" text NULL;""");
    await db.Database.ExecuteSqlRawAsync(
        """ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "Category" text NOT NULL DEFAULT 'General';""");
    await db.Database.ExecuteSqlRawAsync(
        """ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT FALSE;""");
    await db.Database.ExecuteSqlRawAsync(
        """ALTER TABLE "SalesInvoices" ADD COLUMN IF NOT EXISTS "InvoiceNumber" text NOT NULL DEFAULT '';""");
    await db.Database.ExecuteSqlRawAsync(
        """ALTER TABLE "Appointments" ADD COLUMN IF NOT EXISTS "VehicleId" uuid NULL;""");
    await db.Database.ExecuteSqlRawAsync(
        """CREATE INDEX IF NOT EXISTS "IX_Appointments_VehicleId" ON "Appointments" ("VehicleId");""");
    await db.Database.ExecuteSqlRawAsync(
        """UPDATE "Users" SET "Role" = 3 WHERE "Role" = 4;""");
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS "UserPasswordSetupTokens" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "TokenHash" text NOT NULL,
            "ExpiresAtUtc" timestamp with time zone NOT NULL,
            "UsedAtUtc" timestamp with time zone NULL,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_UserPasswordSetupTokens" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_UserPasswordSetupTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
        );
        """);
    await db.Database.ExecuteSqlRawAsync(
        """CREATE INDEX IF NOT EXISTS "IX_UserPasswordSetupTokens_TokenHash" ON "UserPasswordSetupTokens" ("TokenHash");""");
    await db.Database.ExecuteSqlRawAsync(
        """CREATE INDEX IF NOT EXISTS "IX_UserPasswordSetupTokens_UserId" ON "UserPasswordSetupTokens" ("UserId");""");

    try
    {
        await InvoiceNumberBootstrap.BackfillMissingAsync(db);
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Invoice number backfill skipped.");
    }

    if (app.Environment.IsDevelopment())
    {
        try
        {
            await DevAdminBootstrap.EnsureAsync(db);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Development admin bootstrap skipped.");
        }
    }

    try
    {
        await PartVendorBootstrap.RepairAsync(db, assignSoleVendorToOrphans: false);
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Part vendor link repair skipped.");
    }

    try
    {
        var notificationRunner = scope.ServiceProvider.GetRequiredService<INotificationJobRunner>();
        _ = await notificationRunner.RunAsync(forceBypassCooldowns: false);
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Startup notification job skipped.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();