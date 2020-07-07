// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameToResourceConverter.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// NameToResourceConverter deals with internal String-Conversions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace InstantImprovement.SDKControl
{
    /// <summary>
    /// NameToResourceConverter deals with internal String-Conversions
    /// </summary>
    public class NameToResourceConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// Convert String to resource-pointer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String classifier_name = SplitCamelCase(((String)value));
            classifier_name = classifier_name.ToLower().Replace(" ", "_");
            return new Uri("pack://application:,,,/InstantImprovement;component/Resources/" + classifier_name + "." + ((String)parameter));
            // return new Uri(String.Format("pack://application:,,,/{0}.jpg", ((String)value).ToLower()));
        }

        /// <summary>
        /// NotImplemented: Needed to fulfil requirements of IValueConverter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Apply CamelCase-Regex-Parser on string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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