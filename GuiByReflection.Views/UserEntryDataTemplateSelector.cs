using GuiByReflection.ViewModels;
using GuiByReflection.ViewModels.UserEntryVMs;
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
            var vm = item as IUserEntryVM;
            var viewType = SelectViewType(vm);
            return new DataTemplate
            {
                VisualTree = new FrameworkElementFactory(viewType),
            };
        }

        protected virtual Type SelectViewType(IUserEntryVM? vm)
        {
            if (vm == null)
                return typeof(DefaultUserEntryView);

            if (vm is IBoolUserEntryVM)
                return typeof(BoolCheckboxUserEntryView);

            if (vm is IEnumUserEntryVM)
                return typeof(EnumDropdownUserEntryView);

            if (vm is INullableUserEntryVM)
                return typeof(NullableUserEntryView);

            return typeof(DefaultUserEntryView);
        }
    }
}
