using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PatientReferral.Api.Middleware;
using PatientReferral.Application.Interfaces;
using PatientReferral.Application.Services;
using PatientReferral.Infrastructure.Data;
using PatientReferral.Infrastructure.Repositories;
using PatientReferral.Api.Serialization;
using PatientReferral.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IReferralService, ReferralService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientRequestValidator>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "Healthy" })
        : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Database unavailable.");
});

app.MapControllers();

app.Run();

public partial class Program { }
