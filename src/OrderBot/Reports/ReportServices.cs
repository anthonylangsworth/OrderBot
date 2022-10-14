using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Reports
{
    internal static class ReportServices
    {
        public static void AddReports(this IServiceCollection services)
        {
            services.AddSingleton<ToDoListGenerator>();
            services.AddSingleton<ToDoListFormatter>();
        }
    }
}
