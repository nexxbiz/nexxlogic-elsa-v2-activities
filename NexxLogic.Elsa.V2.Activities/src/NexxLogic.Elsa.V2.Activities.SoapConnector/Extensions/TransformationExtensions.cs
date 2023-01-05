using System.Transactions;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NexxLogic.Elsa.V2.Activities.SoapConnector.Exceptions;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Extensions;

public static class TransformationExtensions
{
    public static JToken TransformXmlToJson(string xmlContent)
    {
        try
        {
            var xDoc = XDocument.Parse(xmlContent);
            string json = JsonConvert.SerializeXNode(xDoc);
            return JToken.Parse(json);
        }
        catch (Exception ex)
        {
            throw new TransformationException($"Xml to Json Transformation failed: ${ex.Message}");
        }
    }
    
    public static JToken TransformXmlToJson(XDocument document)
    {
        try
        {
            string json = JsonConvert.SerializeXNode(document);
            return JToken.Parse(json);
        }
        catch (Exception ex)
        {
            throw new TransformationException($"Xml to Json Transformation failed: ${ex.Message}");
        }
    }
    
    public static JToken TransformJsonToXml(string jsonContent)
    {
        try
        {
            var xml = JsonConvert.DeserializeXNode(jsonContent);
            return xml.ToString(SaveOptions.DisableFormatting);
        }
        catch (Exception ex)
        {
            throw new TransformationException($"Json to Xml Transformation failed: ${ex.Message}");
        }
    }
    
    public static JToken TransformJsonToXml(JToken jsonContent)
    {
        try
        {
            var xml = JsonConvert.DeserializeXNode(jsonContent.ToString());
            return xml.ToString(SaveOptions.DisableFormatting);
        }
        catch (Exception ex)
        {
            throw new TransformationException("Json to Xml Transformation failed: ${ex.Message}");
        }
    }
}