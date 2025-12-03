using System.Reflection;
using System.Text;
using FashionApi.Data;
using FashionApi.Repository;
using FashionApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionDocker")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fashion API",
        Version = "v1",
        Description = "API quản lý sản phẩm thời trang, thương hiệu và các thực thể liên quan",
        Contact = new OpenApiContact
        {
            Name = "Tên của bạn",
            Email = "your-email@example.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Thêm JWT Bearer auth to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    options.AddPolicy("AdminOrUser", policy => policy.RequireRole("Admin", "User"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", corsBuilder =>
    {
        corsBuilder
            .WithOrigins("http://localhost:3000", "http://localhost:5173", "https://swagger.io", "https://fashionapp.com") // Thay thêm domains sản xuất
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Quan trọng cho authentication với cookies/credentials
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IMediaServices, MediaServices>();
builder.Services.AddScoped<IMemoryCacheServices, MemoryCacheServices>();
builder.Services.AddScoped<IDanhMucServices, DanhMucServices>();
builder.Services.AddScoped<ISanPhamServices, SanPhamServices>();
builder.Services.AddScoped<INguoiDungServices, NguoiDungServices>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IBinhLuanServices, BinhLuanServices>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion API v1");
        c.RoutePrefix = "swagger";
        c.InjectStylesheet("/swagger-ui/swagger-ui.css");
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetService<ILogger<Program>>();
            logger.LogError(error.Error, "Lỗi không xử lý được: {ErrorMessage}", error.Error.Message);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Lỗi máy chủ nội bộ",
                Detail = app.Environment.IsDevelopment() ? error.Error.Message : "Đã xảy ra lỗi không mong muốn"
            });
        }
    });
});

app.MapGet("/health", async (ApplicationDbContext context, ILogger<Program> logger) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation("Kiểm tra kết nối cơ sở dữ liệu: {CanConnect}", canConnect);
        return Results.Ok(new { DatabaseConnected = canConnect, Status = "Khỏe mạnh" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi kiểm tra kết nối cơ sở dữ liệu: {ErrorMessage}", ex.Message);
        return Results.StatusCode(500);
    }
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Áp dụng migration thành công");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi áp dụng migration: {ErrorMessage}", ex.Message);
        throw;
    }
}

app.Run("http://0.0.0.0:5083");
