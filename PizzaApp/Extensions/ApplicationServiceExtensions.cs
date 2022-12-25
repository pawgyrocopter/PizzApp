using Domain.Helpers;
using Domain.Interfaces.IRepository;
using Domain.Interfaces.IServices;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace PizzaApp.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPizzaRepository, PizzaRepository>();
        services.AddScoped<ITopingRepository, TopingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPizzaService, PizzaService>();
        services.AddScoped<ITopingService, TopingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddSignalR();
        // services.AddSingleton<StateCheckerService>();
        var connectionString = configuration.GetConnectionString("PostgreConnection");
        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }
}