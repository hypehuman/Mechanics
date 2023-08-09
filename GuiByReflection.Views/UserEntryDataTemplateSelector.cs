using GuiByReflection.ViewModels;
using GuiByReflection.Views.UserEntryViews;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuiByReflection.Views
{
    public class UserEntryDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parameterVM = item as IParameterVM;
            var viewType = SelectViewType(parameterVM?.ParameterType);
            return new DataTemplate
            {
                VisualTree = new FrameworkElementFactory(viewType),
            };
        }

        protected virtual Type SelectViewType(Type? parameterType)
        {
            if (parameterType == null)
                return typeof(DefaultUserEntryView);

            if (parameterType == typeof(bool))
                return typeof(BoolCheckboxUserEntryView);

            if (parameterType.IsAssignableTo(typeof(Enum)))
                return typeof(EnumDropdownUserEntryView);

            return typeof(DefaultUserEntryView);
        }
    }
}
