using System.Reflection;
using WebMonitor.Model;

namespace WebMonitor.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class FeatureGuardAttribute : Attribute
{
    private readonly PropertyInfo _propertyInfo;

    public FeatureGuardAttribute(string propertyName)
    {
        _propertyInfo = typeof(AllowedFeatures).GetProperty(propertyName) ??
                        throw new ArgumentException($"Property {propertyName} does not exist in SupportedFeatures");
    }

    public bool GetValue(AllowedFeatures allowedFeatures)
    {
        if (_propertyInfo.GetValue(allowedFeatures) is bool b)
        {
            return b;
        }

        return false;
    }
}