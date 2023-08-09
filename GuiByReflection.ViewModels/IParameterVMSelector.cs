using System.Reflection;

namespace GuiByReflection.ViewModels;

public interface IParameterVMSelector
{
    IParameterVM SelectParameterVM(MethodBase method, int parameterIndex, ParameterInfo parameter);
}
