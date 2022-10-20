using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Reports
{
    internal static class ReportExtensions
    {
        public static void AddReports(this IServiceCollection services)
        {
            services.AddSingleton<ToDoListGenerator>();
            services.AddSingleton<ToDoListFormatter>();
        }
    }
}
