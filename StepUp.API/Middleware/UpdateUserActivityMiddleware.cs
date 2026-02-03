using Microsoft.EntityFrameworkCore;
using StepUp.Infrastructure.Data;
using System.Text.Json;

namespace StepUp.API.Middleware;

public class UpdateUserActivityMiddleware
{
    private readonly RequestDelegate _next;

    public UpdateUserActivityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Validează user și actualizează LastActiveAt pentru user-ul din request
        var shouldContinue = await ValidateAndUpdateUserActivity(context, serviceProvider);
        if (!shouldContinue)
        {
            return;
        }

        await _next(context);
    }

    private async Task<bool> ValidateAndUpdateUserActivity(HttpContext context, IServiceProvider serviceProvider)
    {
        try
        {
            Guid? userId = null;

            // Încearcă să obțină userId din query string
            if (context.Request.Query.TryGetValue("userId", out var userIdString))
            {
                if (Guid.TryParse(userIdString.ToString(), out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Dacă nu s-a găsit în query string, încearcă din header
            if (!userId.HasValue && context.Request.Headers.TryGetValue("X-User-Id", out var headerUserId))
            {
                if (Guid.TryParse(headerUserId.ToString(), out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Dacă am găsit un userId, actualizează LastActiveAt
            if (userId.HasValue)
            {
                // Creează un scope nou pentru DbContext pentru a evita problemele de concurență
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var user = await dbContext.Users
                    .Where(u => u.Id == userId.Value)
                    .Select(u => new { u.Id, u.IsSuspended })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    await WriteAccountDisabledResponse(context, "deleted", "Contul a fost șters.");
                    return false;
                }

                if (user.IsSuspended)
                {
                    await WriteAccountDisabledResponse(context, "suspended", "Contul este suspendat.");
                    return false;
                }

                await UpdateLastActiveAt(userId.Value, dbContext);
            }
        }
        catch (Exception)
        {
            // Ignoră erorile pentru a nu afecta request-ul principal
            // Log error dacă este necesar în producție
        }

        return true;
    }

    private async Task UpdateLastActiveAt(Guid userId, ApplicationDbContext dbContext)
    {
        try
        {
            // Folosește ExecuteUpdate pentru performanță maximă (EF Core 7+)
            // Dacă nu este disponibil, folosește metoda tradițională
            await dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.LastActiveAt, DateTime.UtcNow));
        }
        catch
        {
            // Fallback la metoda tradițională dacă ExecuteUpdate nu este disponibil
            try
            {
                var user = await dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    user.LastActiveAt = DateTime.UtcNow;
                    dbContext.Users.Update(user);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // Ignoră erorile pentru actualizarea LastActiveAt
            }
        }
    }

    private static async Task WriteAccountDisabledResponse(HttpContext context, string status, string message)
    {
        context.Response.StatusCode = status == "suspended" ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Account-Status"] = status;
        var payload = JsonSerializer.Serialize(new { success = false, message });
        await context.Response.WriteAsync(payload);
    }
}

