using _10xWarehouseNet.Clients;
using _10xWarehouseNet.Db;
using Microsoft.EntityFrameworkCore;
using _10xWarehouseNet.Services;


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

builder.Services.AddScoped<IOrganizationService, OrganizationService>();

builder.Services.AddControllers();
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

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
