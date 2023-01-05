# Documentation

## Running the samples project

1. Add wsdl url in the app settings like

```json
  "SoapActivitySettings": [
    {
      "WsdlEndpoint": "http://www.dneonline.com/calculator.asmx?WSDL"
    }
```
2. Debug the application
3. Import the workflow from Workflows folder ( or create a new workflow)
4. Make a GET request to the localhost://{PORT}/test

Flow:
- generate soap handler activities at the startup based on soap service wsdl url
  - this happens in the SoapActivityTypeProvider
  - some of the properties like TargetNamespace, BaseUrl are injected automatically ( they are not exposed in the constructed activity as they are read from the wsdl itself);
  

Issue:
SoapHandler.cs activity ( NexxLogic.Elsa.V2.Activities.SoapConnector)
- the soap url, action etc does not get injected in the composite activity - all of these are coming from the soap interpreter which reads the wsdl file; however the soap body ( that comes from the workflow definition itself) has value and works correctly.