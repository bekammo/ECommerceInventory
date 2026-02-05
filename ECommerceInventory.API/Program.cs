using ECommerceInventory.API.Middleware;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Discounts;
using ECommerceInventory.Infrastructure.BackgroundServices;
using ECommerceInventory.Infrastructure.Data;
using ECommerceInventory.Infrastructure.Repositories;
using ECommerceInventory.Infrastructure.Security;
using ECommerceInventory.Infrastructure.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false; // Enable automatic 400 responses for invalid models
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "ECommerce Inventory API";
        document.Info.Version = "v1";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        var securityRequirement = new OpenApiSecurityRequirement();
        securityRequirement.Add(new OpenApiSecuritySchemeReference("Bearer"), []);
        document.Security ??= [];
        document.Security.Add(securityRequirement);

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<ISessionRepository, EfSessionRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IOutboxRepository, EfOutboxRepository>();

builder.Services.AddSingleton<PaymentQueue>();
builder.Services.AddSingleton<DiscountCardFactory>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddHostedService<PaymentProcessingService>();
builder.Services.AddHostedService<OutboxEventPublisherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ECommerce Inventory API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "ECommerce Inventory API - Swagger";
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseTokenAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
