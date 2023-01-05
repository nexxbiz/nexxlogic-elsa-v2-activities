using System.Runtime.CompilerServices;
using Elsa;
using Elsa.Builders;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Metadata;
using Elsa.Providers.Activities;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Activities;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Constants;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Settings;
using NexxLogic.SoapServiceInterpreter;
using NexxLogic.SoapServiceInterpreter.Entities;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.ActivityProviders;

public class SoapActivityTypeProvider : IActivityTypeProvider
{
    private readonly IActivityActivator activityActivator;
    private readonly IDescribesActivityType describesActivityType;
    private readonly SoapActivityOptions soapServiceOptions;
    private readonly ISoapInterpreter soapInterpreter;

    public SoapActivityTypeProvider(IActivityActivator activityActivator, IOptions<SoapActivityOptions> options,IDescribesActivityType describesActivityType,  ISoapInterpreter soapInterpreter)
    {
        this.activityActivator = activityActivator;
        this.describesActivityType = describesActivityType;
        soapServiceOptions = options.Value;
        this.soapInterpreter = soapInterpreter;
    }

    public async ValueTask<IEnumerable<ActivityType>> GetActivityTypesAsync(CancellationToken cancellationToken) =>
        await GetActivityTypesInternal(cancellationToken).ToListAsync(cancellationToken);

    private async IAsyncEnumerable<ActivityType> GetActivityTypesInternal(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var soapService in soapServiceOptions)
        {
            var soapEntity = soapInterpreter.Read(soapService.WsdlEndpoint);
            foreach (var soapMethod in soapEntity.Operations)
            {
                yield return await CreateSoapActivity(soapMethod, soapEntity.ServiceName, soapEntity.TargetNamespace, soapEntity.EndpointAddress, cancellationToken);
            }
        }
    }

    private ActivityInputDescriptor[] GetFilteredInputDescriptors(ActivityInputDescriptor[] inputDescriptors)
    {
        var filtered = inputDescriptors.ToList().FindAll(e => e.Name != nameof(SoapHandler.BaseUrl) 
                                                              && e.Name!= nameof(SoapHandler.HeaderTargetNamespace)
                                                              && e.Name!= nameof(SoapHandler.BodyTargetNamespace)
                                                              && e.Name!= nameof(SoapHandler.Action)
                                                              );

        return filtered.ToArray();
    }
    
    private async Task<ActivityType>  CreateSoapActivity(ServiceOperation serviceOperation, string category, string targetNamespace, string endpoint, CancellationToken cancellationToken)
    {
        var displayName = serviceOperation.Name.Humanize();
        var type = $"{category.Dehumanize()}-{serviceOperation.Action}";
        var des = await describesActivityType.DescribeAsync<SoapHandler>(cancellationToken);
        
        // Create descriptor
        ValueTask<ActivityDescriptor> CreateDescriptorAsync()
        {
            var descriptor = new ActivityDescriptor
            {
                Type = type,
                DisplayName = displayName,
                Description = serviceOperation.Description,
                Category = category.Humanize(),
                Outcomes = new[] {ActivityOutcomeName.Success, ActivityOutcomeName.Fail},
                Traits = ActivityTraits.Action,
                InputProperties = GetFilteredInputDescriptors(des.InputProperties),
                OutputProperties = GetActivityOutput(),
            };
            return new ValueTask<ActivityDescriptor>(descriptor);
        }

        // Activate activity
        async ValueTask<IActivity> ActivateActivityAsync(ActivityExecutionContext executionContext)
        {
            var activity = await activityActivator.ActivateActivityAsync<SoapHandler>(executionContext, cancellationToken);

            activity.Action = serviceOperation.Action;
            activity.BaseUrl = new Uri(endpoint);
            activity.BodyTargetNamespace = targetNamespace;
            activity.HeaderTargetNamespace = targetNamespace;

            return activity;
        }
        
        return new ActivityType
        {
            TypeName = type,
            Type = typeof(SoapHandler),
            Description = $"Soap Client Request for method {serviceOperation.Name}",
            DisplayName = displayName,
            ActivateAsync = ActivateActivityAsync,
            Attributes = new Dictionary<string, object>(),
            DescribeAsync = CreateDescriptorAsync
        };
    }
    
    private static ActivityOutputDescriptor[] GetActivityOutput()
    {
        return new[]
        {
            new ActivityOutputDescriptor
            (
                "Output",
                typeof(JToken),
                "The response of the soap call."
            )
        };
    }
}