using System.Globalization;

namespace Monivise.App.Utilities;

public static class CurrencyFormatter
{
    public static string Format(decimal amount)
    {
        return amount.ToString("C0", new CultureInfo("en-NG"));
    }
}