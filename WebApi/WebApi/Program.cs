using Converter.Api.Service.Converters;
using Converter.Api.Service.Converters.Interfaces;
using Converter.Api.Service.Repositories;
using Converter.Api.Service.Repositories.Interfaces;
using Converter.Api.Service.Services;
using Converter.Api.Service.Services.Interfaces;
using Converter.Api.Service.Settings;
using Hangfire;
using Hangfire.MemoryStorage;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
 
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<AddFileUploadParams>();
});

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHangfire(config => config
    .UseMemoryStorage());

builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IFileRepository, FileRepository>();
builder.Services.AddTransient<IConverterFactory, ConverterFactory>();
builder.Services.AddTransient<IConverter, HtmlToPdfConverter>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHangfireDashboard();
app.UseHangfireServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
