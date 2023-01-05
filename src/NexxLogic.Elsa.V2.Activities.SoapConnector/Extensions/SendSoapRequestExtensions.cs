using System.Linq.Expressions;
using Elsa.Builders;
using Elsa.Services.Models;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Activities;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Extensions;

public static class SendSoapRequestExtensions
{
    public static ISetupActivity<SendSoapRequest> WithUrl(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<Uri?>> value)
    {
        return activity.Set<Uri>((Expression<Func<SendSoapRequest, Uri>>) (x => x.BaseUrl), value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithAction(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<string?>> value)
    {
        return activity.Set((Expression<Func<SendSoapRequest, string>>) (x => x.Action), value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithBody(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<string?>> value)
    {
        return activity.Set((Expression<Func<SendSoapRequest, string>>) (x => x.SoapBody), value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithHeader(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<string?>> value)
    {
        return activity.Set((Expression<Func<SendSoapRequest, string>>) (x => x.SoapHeader), value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithBodyTargetNamespace(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<string?>> value)
    {
        return activity.Set((Expression<Func<SendSoapRequest, string>>) (x => x.SoapBodyTargetNamespace), value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithEnabledBodyTargetNamespace(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<bool>> value)
    {
        return activity.Set(x => x.WithBodyTargetNamespace, value);
    }
    
    public static ISetupActivity<SendSoapRequest> WithEnabledHeaderTargetNamespace(
        this ISetupActivity<SendSoapRequest> activity,
        Func<ActivityExecutionContext, ValueTask<bool>> value)
    {
        return activity.Set(x => x.WithHeaderTargetNamespace, value);
    }
    
}