using System.Runtime.Serialization;

namespace NexxLogic.Elsa.V2.Activities.SoapConnector.Exceptions;

[Serializable]
public class TransformationException : Exception
{
    public TransformationException(string message) : base(message)
    {
    }
    
    protected TransformationException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}