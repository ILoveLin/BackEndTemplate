namespace VideoShare_BackEnd.Models.Attributes;

[Serializable]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiVersionToLatestAttribute:Attribute
{
    public readonly double? version;
    

    public ApiVersionToLatestAttribute(double _version)
    {
        version = _version;
    }
}