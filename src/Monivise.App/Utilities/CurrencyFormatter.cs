using System.Globalization;

namespace Monivise.App.Utilities;

public static class CurrencyFormatter
{
    public static string Format(decimal amount, string currencyCode)
    {
        var culture = currencyCode switch
        {
            "NGN" => new CultureInfo("en-NG"),
            "USD" => new CultureInfo("en-US"),
            "GBP" => new CultureInfo("en-GB"),
            _ => CultureInfo.InvariantCulture
        };
        return amount.ToString("C0", culture);
    }
}