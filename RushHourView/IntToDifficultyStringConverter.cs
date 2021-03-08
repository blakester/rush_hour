using System;
using System.Windows.Data;

namespace RushHourView
{
    public class IntToDifficultyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int difficulty = (int)value;

            switch (difficulty)
            {
                case 1:
                    return "Beginner";
                case 2:
                    return "Intermediate";
                case 3:
                    return "Advanced";
                case 4:
                    return "Expert";
                case 5:
                    return "Grand Master";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
