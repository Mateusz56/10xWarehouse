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

builder.Services.AddSingleton(new Supabase.Client(supabaseUrl, supabaseServiceRoleKey));

builder.Services.AddScoped<SupabaseUsers>(provider =>
{
    var supabaseClient = provider.GetRequiredService<Supabase.Client>();
    return new SupabaseUsers(supabaseClient, supabaseServiceRoleKey);
});

// Add authentication services
builder.Services.AddScoped<SupabaseJwtAuthenticationService>(provider =>
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
builder.Services.AddSwaggerGen();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:4321")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

// Order matters: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
