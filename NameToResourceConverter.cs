using InstantImprovement.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace InstantImprovement
{
    public class NameToResourceConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String classifier_name = SplitCamelCase(((String)value));
            classifier_name = classifier_name.ToLower().Replace(" ", "_");
            return new Uri("pack://application:,,,/InstantImprovement;component/Resources/" + classifier_name + "." +((String) parameter));
            //return new Uri(String.Format("pack://application:,,,/{0}.jpg", ((String)value).ToLower()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string SplitCamelCase(String str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }

}
