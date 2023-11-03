using System;
using System.Linq;
using System.Reflection;

namespace Documentation;

public class Specifier<T> : ISpecifier
{
    Type type = typeof(T);
    public string GetApiDescription()
    {
        return type.GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
    }

    public string[] GetApiMethodNames()
    {
        return type.GetMethods()
            .Where(x => x.GetCustomAttributes().OfType<ApiMethodAttribute>().Any())
            .Select(x => x.Name)
            .ToArray();        
    }

    public string GetApiMethodDescription(string methodName)
    {
        var method = type.GetMethod(methodName);
        if (method == null) return null;
        return method.GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
    }

    public string[] GetApiMethodParamNames(string methodName)
    {
        var method = type.GetMethod(methodName);
        if (method == null) return null;
        return method.GetParameters().Select(x => x.Name).ToArray();
    }

    public string GetApiMethodParamDescription(string methodName, string paramName)
    {
        var method = type.GetMethod(methodName);
        if (method == null) return null;

        var param = method.GetParameters().Where(x => x.Name == paramName);
        if (!param.Any()) return null;

        return param.First().GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
    }

    public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
    {
        var result = new ApiParamDescription() { ParamDescription = new CommonDescription(paramName) };

        var method = type.GetMethod(methodName);
        if (method == null) return result;

        var param = method.GetParameters().Where(x => x.Name == paramName).FirstOrDefault();
        if (param == null) return result;

        result.ParamDescription.Description = 
            param.GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
        result.MaxValue = param.GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MaxValue;
        result.MinValue = param.GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MinValue;
        var required = param.GetCustomAttributes().OfType<ApiRequiredAttribute>().FirstOrDefault();
        if (required != null) result.Required = required.Required;

        return result;
    }

    public ApiMethodDescription GetApiMethodFullDescription(string methodName)
    {    
        var method = type.GetMethod(methodName);
        if (method == null || !method.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) 
            return null;

        var result = new ApiMethodDescription() {
            MethodDescription = new CommonDescription(methodName, GetApiMethodDescription(methodName)),
            ParamDescriptions = GetApiMethodParamNames(methodName).Select(x => GetApiMethodParamFullDescription(methodName, x)).ToArray() 
        };

        var returnParametr = method.ReturnParameter;
        bool set = false;
        var returnParametrFullDescription = new ApiParamDescription() { ParamDescription = new CommonDescription() };

        var description = returnParametr.GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault();
        if (description != null)
        {
            returnParametrFullDescription.ParamDescription.Description = description.Description;
            set = true;
        }

        var validationDescription = returnParametr.GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault();
        if (validationDescription != null)
        {
            returnParametrFullDescription.MaxValue = validationDescription.MaxValue;
            returnParametrFullDescription.MinValue = validationDescription.MinValue;
            set = true;
        }

        var required = returnParametr.GetCustomAttributes().OfType<ApiRequiredAttribute>().FirstOrDefault();
        if (required != null)
        {
            returnParametrFullDescription.Required = required.Required;
            set = true;
        }
        
        if (set) result.ReturnDescription = returnParametrFullDescription;
        return result;
    }
}