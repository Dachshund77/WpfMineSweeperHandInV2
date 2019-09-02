using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfMineSweeperHandInV2
{
    /// <summary>
    /// Converter that expects two integer, that will substract the second from the first.
    /// Will Also convert it to String
    /// </summary>
    class SubstractionMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("CONVERTER CALLED");
            Console.WriteLine(values[0].ToString());
            Console.WriteLine(values[1].ToString());
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return 0;
            }
            else
            {
                int firstInt = (int)values[0];
                int secondInt = (int)values[1];
                return (firstInt - secondInt).ToString();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
