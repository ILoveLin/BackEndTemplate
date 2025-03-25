namespace VideoShare_BackEnd.Models.Attributes
{
    //标识一个方法或类，表示该方法或类适用于所有版本。
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllApiVersionsAttribute : Attribute
    {
    }
}