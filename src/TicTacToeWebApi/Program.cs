using Microsoft.EntityFrameworkCore;
using TicTacToeWebApi.Models;
using TicTacToeWebApi.Services;
using TicTacToeWebApi.Services.Interfaces;

namespace TicTacToeWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            string connection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationContextService>(options => options.UseNpgsql(connection));
            builder.Services.AddScoped<IAutorizationService, AutorizationService>();
            builder.Services.AddSingleton<GameSessions>();
            builder.Services.AddScoped<IPlayingService, PlayingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}