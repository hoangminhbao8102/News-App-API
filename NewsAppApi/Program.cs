using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Data.Mappings;
using NewsAppApi.Services.Implementations;
using NewsAppApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===== Add Controllers =====
builder.Services.AddControllers();

// ===== FluentValidation =====
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserCreateValidator>();

// ===== DbContext =====
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NewsAppDbContext>(options =>
    options.UseSqlServer(connStr));

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ===== Register Services DI =====
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IReadHistoryService, ReadHistoryService>();

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();
