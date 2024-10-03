using BusBooking.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace BusBooking.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BusBooking API", Version = "v1" });
                c.OperationFilter<AddFileUploadOperationFilter>();
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600;
            });
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers()
             .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling =
             Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            // Add services to the container.
            builder.Services.AddDbContext<BusBookingContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("constring")));

            var _jwtsetting = builder.Configuration.GetSection("JWTSetting");
            builder.Services.Configure<JWTSetting>(_jwtsetting);
            builder.Services.AddScoped<RazorpayService>();

            var authkey = builder.Configuration.GetValue<string>("JWTSetting:securitykey");

            builder.Services.AddAuthentication(item =>
            {
                item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(item =>
            {

                item.RequireHttpsMetadata = true;
                item.SaveToken = true;
                item.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authkey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            var _dbcontext = builder.Services.BuildServiceProvider().GetService<BusBookingContext>();
            builder.Services.AddSingleton<IRefreshTokenGenerator>(provider => new RefreshTokenGenerator(_dbcontext));

            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins("https://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod();  // Allows any HTTP method (GET, POST, PUT, DELETE, etc.)
                });
            });



            var app = builder.Build();
            //app.UseMiddleware<MethodNotAllowedMiddleware>();


            // Use CORS before routing and authentication
            app.UseCors("AllowSpecificOrigins");
          

            // Ensure requests are routed correctly
            app.UseRouting();

            // Add authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles(); // Enable serving static files from wwwroot

            // Endpoint routing
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

//using BusBooking.Server.Models;
//using BusBooking.Server;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using Microsoft.OpenApi.Models;
//using Microsoft.AspNetCore.Http.Features;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BusBooking API", Version = "v1" });
//    c.OperationFilter<AddFileUploadOperationFilter>();
//});

//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = 104857600; // 100 MB
//});

//builder.Services.AddDbContext<BusBookingContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("constring")));

//// Configure JWT Authentication
//var jwtSettings = builder.Configuration.GetSection("JWTSetting");
//builder.Services.Configure<JWTSetting>(jwtSettings);

//var authKey = builder.Configuration.GetValue<string>("JWTSetting:securitykey");

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = true;
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ClockSkew = TimeSpan.Zero
//    };
//});

//// Register other services
//var serviceProvider = builder.Services.BuildServiceProvider();
//builder.Services.AddSingleton<IRefreshTokenGenerator>(provider => new RefreshTokenGenerator(serviceProvider.GetRequiredService<BusBookingContext>()));


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins",
//       policyBuilder =>
//        {
//            policyBuilder.WithOrigins("https://localhost:5173")
//                         .AllowAnyHeader()
//                         .AllowAnyMethod();
//        });
//});

////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowSpecificOrigins",
////        policyBuilder =>
////        {
////            policyBuilder.WithOrigins("https://localhost:5173")
////                         .AllowAnyHeader()
////                         .AllowAnyMethod()
////                         .AllowCredentials() // If you are handling authentication
////                         .WithExposedHeaders("Content-Disposition"); // Expose headers if needed for file downloads
////        });
////});

//var app = builder.Build();

//app.UseCors("AllowSpecificOrigins");

//app.UseDefaultFiles();
//app.UseStaticFiles();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthentication(); // Ensure Authentication is used
//app.UseAuthorization();

//app.MapControllers();
//app.MapFallbackToFile("/index.html");

//app.Run();
