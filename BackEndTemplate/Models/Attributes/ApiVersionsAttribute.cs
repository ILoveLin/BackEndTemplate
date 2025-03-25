namespace BackEndTemplate.Models.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionsAttribute : Attribute
    {
        public readonly double[] versions;

        public ApiVersionsAttribute(double[] _versions)
        {
            versions = _versions;
        }

        public ApiVersionsAttribute(double _version)
        {
            versions = [_version];
        }
    }
}