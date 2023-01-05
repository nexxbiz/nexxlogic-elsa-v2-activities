using Elsa.Options;
using Microsoft.Extensions.DependencyInjection;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Activities;
using NexxLogic.Elsa.V2.Activities.SoapConnector.ActivityProviders;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Settings;
using NexxLogic.SoapConnector.Client;
using NexxLogic.SoapServiceInterpreter;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Extensions;

public static class ServiceCollectionExtensions
{
    public static ElsaOptionsBuilder AddSoapActivitiesProvider(this ElsaOptionsBuilder optionsBuilder,
        Action<SoapActivityOptions>? configureOptions = default)
    {
        optionsBuilder.Services.AddActivityTypeProvider<SoapActivityTypeProvider>();

        optionsBuilder.Services.AddSingleton<ISoapInterpreter, SoapInterpreter>();
        optionsBuilder.Services.AddSingleton<ISoapClient, SoapClient>();

        if (configureOptions != null)
            optionsBuilder.Services.Configure(configureOptions);
        
        return optionsBuilder;
    }

    public static ElsaOptionsBuilder AddSoapConnector(this ElsaOptionsBuilder options)
    {
        options.Services.AddSoapConnectorServices();
        options.AddSoapConnectorActivity();
        return options;
    }

    private static ElsaOptionsBuilder AddSoapConnectorActivity(this ElsaOptionsBuilder services) =>
        services.AddActivity<SendSoapRequest>()
            .AddActivity<SoapHandler>();
    
    private static IServiceCollection AddSoapConnectorServices(this IServiceCollection services)
    {
        services
            .AddSingleton<ISoapClient, SoapClient>();
        return services;
    }
}