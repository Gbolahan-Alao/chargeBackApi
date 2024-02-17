using Fileuploads.Services;
using Fileuploads.Services.DatabaseServices;
using Fileuploads.Services.FileUploadServices;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Set the ExcelPackage license context
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")
        ?? throw new InvalidOperationException("Connection string is not found"));
});

builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<TeamaptFileUploadService>();
builder.Services.AddScoped<PalmpayDatabaseServices>();
builder.Services.AddScoped<TeamaptDatabaseServices>();
builder.Services.AddScoped<FairmoneyDatabaseServices>();
builder.Services.AddScoped<PalmpayFileUploadService>();
builder.Services.AddScoped<FairmoneyFileUploadService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<ExcelDataService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var MyAllowCorsSpecificOrigins = "AllowCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPermission", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("ClientPermission");
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.MapControllers();

app.Run();
