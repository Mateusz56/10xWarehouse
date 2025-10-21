using _10xWarehouseNet.Clients;
using _10xWarehouseNet.Db;
using Microsoft.EntityFrameworkCore;
using _10xWarehouseNet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using _10xWarehouseNet.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var supabaseUrl = builder.Configuration["Supabase:Url"]!;
var supabaseServiceRoleKey = builder.Configuration["Supabase:ServiceRoleKey"]!;

builder.Services.AddScoped(provider => new Supabase.Client(supabaseUrl, supabaseServiceRoleKey));

builder.Services.AddScoped(provider =>
{
    var supabaseClient = provider.GetRequiredService<Supabase.Client>();
    return new SupabaseUsers(supabaseClient, supabaseServiceRoleKey);
});

// Add authentication services
builder.Services.AddScoped(provider =>
{
    var supabaseClient = provider.GetRequiredService<Supabase.Client>();
    return new SupabaseJwtAuthenticationService(supabaseClient, supabaseServiceRoleKey);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<JwtBearerOptions, SupabaseJwtAuthenticationHandler>(
        JwtBearerDefaults.AuthenticationScheme, 
        options => { });

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOrMember", policy =>
        policy.Requirements.Add(new DatabaseRoleRequirement(DatabaseRoleType.OwnerOrMember)));
    
    options.AddPolicy("OwnerOnly", policy =>
        policy.Requirements.Add(new DatabaseRoleRequirement(DatabaseRoleType.OwnerOnly)));
    
    options.AddPolicy("OrganizationMember", policy =>
        policy.Requirements.Add(new DatabaseRoleRequirement(DatabaseRoleType.OrganizationMember)));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, DatabaseRoleAuthorizationHandler>();

builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IProductTemplateService, ProductTemplateService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add HTTP context accessor for authorization handler
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Swagger UI will be available at: https://localhost:7146/swagger/index.html
// To use Bearer token authentication:
// 1. Click the "Authorize" button in Swagger UI
// 2. Enter your JWT token in the format: "Bearer your-jwt-token-here"
// 3. Click "Authorize" to apply the token to all requests
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "10xWarehouse API", 
        Version = "v1",
        Description = "API for 10xWarehouse inventory management system. Use the 'Authorize' button to add your JWT Bearer token."
    });
    
    // Add JWT Bearer token authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:4321", "http://localhost:4322")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "10xWarehouse API v1");
        // Keep default Swagger UI path: /swagger/index.html
        c.DocumentTitle = "10xWarehouse API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide models section by default
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // Expand only the list of operations
    });
}

// Only redirect to HTTPS in production, not in containers
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(MyAllowSpecificOrigins);

// Order matters: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
