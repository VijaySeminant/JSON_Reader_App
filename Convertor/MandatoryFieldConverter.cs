using System.Globalization;

namespace JSON_Reader.Convertor
{
    public class MandatoryFieldConverter : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string[] mandatory_arr_fields = ["Email", "Phone", "Profile"];

            var theValue = (string)value;

            if (mandatory_arr_fields.Contains(theValue))
            {
                return false;
            }
            return true;
        }


        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
