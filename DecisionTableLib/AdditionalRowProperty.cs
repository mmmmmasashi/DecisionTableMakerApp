using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib
{
    public class AdditionalRowProperty
    {
        private PropertyList _propertyList = new PropertyList();

        public IEnumerable<(string, string)> FromProperty(string lastAdditionalSettings)
        {
            string settingStr = lastAdditionalSettings;
            settingStr ??= "結果|実施日||実施者||結果"; // デフォルト値

            return _propertyList
                .FromPropertyString(settingStr)
                .ToList();
        }


        public string ToPropertyString(List<(string Col1Text, string Col2Text)> settings)
        {
            return _propertyList.ToPropertyString(settings);
        }

    }
}
