using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.Omid;

public static class OmidCustomerConfiguration
{
    public static IServiceCollection AddCustomerOmidConfiguration(this IServiceCollection services, DTOServerInfo customer)
    {
        // Use the same startup snapshot as BMain and the invoice service.
        // Saving the settings form takes effect after restarting the application.
        services.AddSingleton<IOptions<OmidConfig>>(Options.Create(OmidConfig.FromCustomer(customer)));
        return services;
    }
}
