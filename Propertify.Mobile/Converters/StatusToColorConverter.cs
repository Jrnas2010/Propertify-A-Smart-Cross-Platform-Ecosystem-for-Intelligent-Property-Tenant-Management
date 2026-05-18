using System.Globalization;

namespace Propertify.Mobile.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value?.ToString() ?? "";
            return status switch
            {
                "Paid"       or "Active"     or "Done"       => Color.FromArgb("#10b981"),
                "Unpaid"     or "Pending"                    => Color.FromArgb("#f59e0b"),
                "Overdue"    or "Expired"    or "Urgent"     => Color.FromArgb("#ef4444"),
                "InProgress" or "In Progress"                => Color.FromArgb("#3b82f6"),
                "High"                                       => Color.FromArgb("#f97316"),
                _ => Color.FromArgb("#64748b")
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
