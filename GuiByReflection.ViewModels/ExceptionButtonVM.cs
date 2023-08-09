using System.Reflection;

namespace GuiByReflection.ViewModels;

public interface IExceptionButtonVM
{
    string ShortMessage { get; }
    string Details { get; }
}

public class ExceptionButtonVM : IExceptionButtonVM
{
    private readonly Exception _exception;

    public ExceptionButtonVM(Exception exception)
    {
        _exception = exception;
    }

    public string ShortMessage
    {
        get
        {
            Exception informativeEx = _exception;
            if (informativeEx is TargetInvocationException tie)
            {
                // Otherwise the message will be vague: "Exception has been thrown by the target of an invocation."
                informativeEx = tie.InnerException ?? tie;
            }
            return informativeEx.Message;
        }
    }

    public string Details => _exception.ToString();
}
