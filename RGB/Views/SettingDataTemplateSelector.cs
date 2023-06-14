using RGB.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Views
{
    class SettingDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleSettingTemplate { get; set; }
        public DataTemplate ColorSettingTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is SingleValueSettingVM)
            {
                return SingleSettingTemplate;
            }

            return ColorSettingTemplate;
        }
    }
}
