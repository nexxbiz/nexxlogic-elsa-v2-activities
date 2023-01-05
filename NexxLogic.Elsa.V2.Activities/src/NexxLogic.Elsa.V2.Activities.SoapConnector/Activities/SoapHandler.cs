using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Attributes;
using Elsa.Builders;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Constants;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Extensions;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Models;
using NexxLogic.SoapConnector.Enums;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Activities;

[Action(
    Category = "Soap",
    DisplayName = "Soap Handler",
    Description = "Transforms and executes soap services",
    Outcomes = new[] {ActivityOutcomeName.Success, ActivityOutcomeName.Fail}
)]
//[Browsable(false)]
public class SoapHandler : CompositeActivity
{
    private const string SendSoapRequestActivityName = "SoapService";
    private const string SoapActivityResponseContentPropertyName = "ResponseContent";
    private const string SoapActivityResponsePropertyName = "Response";

    [ActivityInput(Hint = "The URL to send the HTTP request to.",
        SupportedSyntaxes = new[] {SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public Uri? BaseUrl
    {
        get => GetState<Uri>();
        set => SetState(value);
    }
    
    [ActivityInput(Hint = "The URL to send the HTTP request to.",
        UIHint = ActivityInputUIHints.SingleLine,
        SupportedSyntaxes = new[] {SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public ICollection<int>? StatusCodesSuccess
    {
        get => GetState<ICollection<int>>();
        set => SetState(value);
    }
    
    [ActivityInput(
        Hint = "The soap envelop version. 1.1 or 1.2", 
        UIHint = ActivityInputUIHints.Dropdown,
        SupportedSyntaxes = new[] {SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public SoapVersion SoapVersion
    {
        get => GetState<SoapVersion>();
        set => SetState(value);
    }
    
    [ActivityInput(
        Hint ="Action",
        UIHint = ActivityInputUIHints.SingleLine,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public string? Action
    {
        get => GetState<string>();
        set => SetState(value);
    }

    #region Soap Body

    [ActivityInput(
        Hint =
            "Body Target Namespace",
        Category = "Soap Body Settings", 
        UIHint = ActivityInputUIHints.SingleLine,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public string? BodyTargetNamespace
    {
        get => GetState<string>();
        set => SetState(value);
    }
    
    [ActivityInput(
        Hint =
            "Soap Body as JSON",
        Category = "Soap Body Settings", 
        UIHint = ActivityInputUIHints.MultiText,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid, SyntaxNames.Json})]
    public JToken SoapBody
    {
        get => GetState<JToken>();
        set => SetState(value);
    }
    #endregion

    #region Soap Header

    [ActivityInput(
        Hint =
            "Soap Body as JSON",
        Category = "Soap Header Settings", 
        UIHint = ActivityInputUIHints.MultiText,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid, SyntaxNames.Json})]
    public JToken? SoapHeader
    {
        get => GetState<JToken>();
        set => SetState(value);
    }
    
    [ActivityInput(
        Hint =
            "Header Target Namespace",
        Category = "Soap Header Settings", 
        UIHint = ActivityInputUIHints.SingleLine,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid})]
    public string? HeaderTargetNamespace
    {
        get => GetState<string>();
        set => SetState(value);
    }
    #endregion

    public override void Build(ICompositeActivityBuilder builder)
    {
        builder.StartWith<SendSoapRequest>(SetupSoapCall)
            .WithName(SendSoapRequestActivityName)
            .Then<If>(SetupIsCallSuccessful,
                soapCallBuilder =>
                {
                    soapCallBuilder.When(OutcomeNames.False).Finish(activity =>
                    {
                        activity.WithOutcome(ActivityOutcomeName.Fail)
                            .WithOutput(async context =>
                            {
                                var response = await context.GetNamedActivityPropertyAsync<SoapResponseModel>(
                                    SendSoapRequestActivityName,
                                    SoapActivityResponsePropertyName, context.CancellationToken)!;

                                var responseContent = await context.GetNamedActivityPropertyAsync<string>(
                                    SendSoapRequestActivityName,
                                    SoapActivityResponseContentPropertyName, context.CancellationToken)!;
                                
                                var result = new JObject();
                                result.Add(SoapActivityResponsePropertyName, JToken.FromObject(response));

                                var transformedObject = JToken.Parse("{}");
                                try
                                {
                                    transformedObject =
                                        TransformationExtensions.TransformXmlToJson(responseContent);
                                }
                                catch (Exception ex)
                                {
                                    transformedObject = new JObject("TransformationError", JsonConvert.SerializeObject(ex));
                                }

                                result.Add("ResponseContentXml", responseContent);
                                result.Add("ResponseContentJson", transformedObject);

                                return result;
                            });

                    });

                    soapCallBuilder.When(OutcomeNames.True)
                        .Finish( activity =>
                        {
                            activity.WithOutcome(OutcomeNames.Done)
                                .WithOutput(async context =>
                                {
                                    var response = await context.GetNamedActivityPropertyAsync<SoapResponseModel>(
                                        SendSoapRequestActivityName,
                                        SoapActivityResponsePropertyName, context.CancellationToken)!;

                                    var responseContent = await context.GetNamedActivityPropertyAsync<string>(
                                        SendSoapRequestActivityName,
                                        SoapActivityResponseContentPropertyName, context.CancellationToken)!;
                                    var result = new JObject();
                                    result.Add(SoapActivityResponsePropertyName, JToken.FromObject(response));

                                    var transformedObject = JToken.Parse("{}");
                                    try
                                    {
                                        transformedObject =
                                            TransformationExtensions.TransformXmlToJson(responseContent);
                                    }
                                    catch (Exception ex)
                                    {
                                        transformedObject = new JObject("TransformationError", JsonConvert.SerializeObject(ex));
                                    }

                                    result.Add("ResponseContentXml", responseContent);
                                    result.Add("ResponseContentJson", transformedObject);

                                    return result;
                                });
                        });
                });
        
        base.Build(builder);
    }

    private void SetupSoapCall(ISetupActivity<SendSoapRequest> s)
    {
        s.WithUrl(s => new ValueTask<Uri?>(BaseUrl));
        s.Set(p => p.SoapVersion, () => SoapVersion);
        s.WithAction(s => new ValueTask<string?>(Action));
        s.WithEnabledBodyTargetNamespace(context =>
        {
            var withBodyTargetNamespace =  !string.IsNullOrWhiteSpace(BodyTargetNamespace);
            return new ValueTask<bool>(withBodyTargetNamespace);
        });
        s.WithBodyTargetNamespace(_ => new ValueTask<string?>(BodyTargetNamespace));
        
        s.WithBody(context =>
        {
            var xmlBody = TransformationExtensions.TransformJsonToXml(SoapBody);
            return new ValueTask<string?>(xmlBody.ToString());
        });

        if (SoapHeader != null)
        {
            s.WithHeader(context =>
            {
                var xmlHeader = TransformationExtensions.TransformJsonToXml(SoapHeader);
                return new ValueTask<string?>(xmlHeader.ToString());
            });
            
            s.WithEnabledHeaderTargetNamespace(context =>
            {
                var withHeaderTargetNamespace =  !string.IsNullOrWhiteSpace(HeaderTargetNamespace);
                return new ValueTask<bool>(withHeaderTargetNamespace);
            });
        }
    }
    
    private void SetupIsCallSuccessful(ISetupActivity<If> setup)
    {
        setup.Set(p => p.Condition, async context =>
        {
            var response = await context
                .GetNamedActivityPropertyAsync<SoapResponseModel>(
                    SendSoapRequestActivityName,
                    SoapActivityResponsePropertyName);

            return StatusCodesSuccess.Any(t => t == (int)response?.StatusCode);
        });
    }
}