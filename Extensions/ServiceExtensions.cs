using OrderManagementAPI.Repositories;

namespace OrderManagementAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureRepositories(this IServiceCollection services, bool useStoredProcedures = false)
        {
            if (useStoredProcedures)
            {
                // Use stored procedures implementations
                services.AddScoped<IOrderRepository, OrderRepositorySP>();
                services.AddScoped<IOrderDetailRepository, OrderDetailRepositorySP>();
            }
            else
            {
                // Use regular EF Core implementations
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            }
        }
    }
}