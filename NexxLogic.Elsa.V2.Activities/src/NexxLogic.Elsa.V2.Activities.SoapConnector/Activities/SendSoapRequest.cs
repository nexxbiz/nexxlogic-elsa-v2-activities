using System.Xml;
using System.Xml.Linq;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Models;
using NexxLogic.SoapConnector.Client;
using NexxLogic.SoapConnector.Enums;
using NexxLogic.SoapConnector.Extensions;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Activities;

[Activity(
    Category = "Soap",
    DisplayName = "Soap Connector",
    Description = "Executes soap call",
    Outcomes = new[] { OutcomeNames.Done }
)]
public class SendSoapRequest : Activity
{
    public readonly ISoapClient soapClient;
    public SendSoapRequest(ISoapClient soapClient)
    {
        this.soapClient = soapClient;
    }
    
    [ActivityInput(Hint = "The base URL to send the HTTP request to.", 
        SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
    )]
    public Uri BaseUrl { get; set; } = default!;
    
    [ActivityInput(
        UIHint = ActivityInputUIHints.Dropdown,
        Hint = "The soap envelop version. 1.1 or 1.2", 
        Options = new[] { SoapVersion.Soap_11, SoapVersion.Soap_12}
    )]
    public SoapVersion SoapVersion { get; set; } = default!;

    #region Soap Body
    [ActivityInput(Hint = "The soap body as a string.", 
        Category = "Soap Body Settings", 
        UIHint = ActivityInputUIHints.MultiText,
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {  SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
    )]
    public string SoapBody { get; set; } = default!;
    
    [ActivityInput(
        Hint = "Indicates whether the soap body has a target namespace",
        Category = "Soap Body Settings", 
        SupportedSyntaxes = new[] {"Literal", "JavaScript", "Liquid"})]
    public bool WithBodyTargetNamespace { get; set; } = default!;

    [ActivityInput(Hint = "The soap body target namespace",
        DefaultSyntax = SyntaxNames.Literal,
        Category = "Soap Body Settings", 
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid}
    )]
    public string? SoapBodyTargetNamespace { get; set; } = default!;
    
    #endregion

    #region Soap Header
    [ActivityInput(Hint = "The soap header as a string.", 
        UIHint = ActivityInputUIHints.MultiText,
        Category = "Soap Header Settings", 
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {  SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
    )]
    public string? SoapHeader { get; set; } = default!;
    
    [ActivityInput(
        Hint = "Indicates whether the soap header has a target namespace",
        Category = "Soap Header Settings", 
        SupportedSyntaxes = new[] {"Literal", "JavaScript", "Liquid"})]
    public bool WithHeaderTargetNamespace { get; set; } = default!;
    
    [ActivityInput(Hint = "Set the soap header target namespace",
        Category = "Soap Header Settings", 
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid}
    )]
    public string? SoapHeaderTargetNamespace { get; set; } = default!;

    #endregion
    
    [ActivityInput(Hint = "Soap Action",
        DefaultSyntax = SyntaxNames.Literal,
        SupportedSyntaxes = new[] {SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid}
    )]
    public string Action { get; set; } = default!;
    
    [ActivityOutput]
    public string? ResponseContent { get; set; }

    [ActivityOutput] 
    public SoapResponseModel? Response { get; set; }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var result = new HttpResponseMessage();

        var bodyElement = GetXElement(SoapBody);
        var headerElement = GetSoapHeader();
        if (WithBodyTargetNamespace)
        {
            bodyElement = bodyElement.WithTargetNamespace(SoapBodyTargetNamespace);
        }

        if (WithHeaderTargetNamespace)
        {
            headerElement = headerElement.WithTargetNamespace(SoapHeaderTargetNamespace);
        }

        context.JournalData.Add(nameof(SoapBody), bodyElement);
        context.JournalData.Add(nameof(SoapHeader), headerElement);
        
        result = await soapClient.SendAsync(BaseUrl, SoapVersion, bodyElement, headerElement, Action,
            context.CancellationToken);


        ResponseContent = (await result.Content.ReadAsStringAsync()).Trim();

        Response = new SoapResponseModel
        {
            Headers = result.Headers.ToDictionary(x => x.Key, v => v.Value.ToArray()),
            StatusCode = result.StatusCode
        };

        context.JournalData.Add(nameof(ResponseContent), ResponseContent);
        context.JournalData.Add(nameof(Response), Response);
        
        return Done();
    }

    private XElement? GetSoapHeader()
    {
        if (SoapHeader == null)
        {
            return null;
        }

        return GetXElement(SoapHeader);
    }

    private XElement GetXElement(string xmlContent)
    {
        try
        {
            var xDoc = XDocument.Parse(xmlContent);
            return xDoc.Root;
        }
        catch (Exception e)
        {
            throw new XmlException("Given xml string could not be parsed");
        }
    }
}
