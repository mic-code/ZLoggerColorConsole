using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;
using System;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace ZLoggerColorConsole;

public class ColorConsole
{
    static readonly Regex TemplateRegex = new Regex(@"\{([^{}]+?)(?:,([^}]+))?\}", RegexOptions.Compiled);

    static readonly string AnsiReset = "\x1B[0m";
    static readonly string AnsiBrightCyan = "\x1B[38;5;14m";
    static readonly string AnsiGrey = "\x1B[38;5;8m";
    static readonly string AnsiLightGreen = "\x1B[38;5;10m";
    static readonly string AnsiBrightWhite = "\x1B[38;5;15m";
    static readonly string AnsiBrightYellow = "\x1B[38;5;11m";
    static readonly string AnsiBrightRed = "\x1B[38;5;9m";
    //static readonly string AnsiBrightMagenta = "\x1B[38;5;13m";
    static readonly string AnsiBrightOrange = "\x1B[38;5;208m";

    public static void ConfigOption(ILoggingBuilder builder)
    {
        builder.ClearProviders();

        builder.SetMinimumLevel(LogLevel.Trace);
        builder.AddZLoggerInMemory(
        (option, provider) =>
        {
            option.UseFormatter(() => new CLEFMessageTemplateFormatter());

        }, processor =>
        {
            processor.MessageReceived += msg =>
            {
                var jsonObject = JsonNode.Parse(msg)?.AsObject();
                if (jsonObject != null)
                {
                    var timestamp = jsonObject["@t"]?.GetValue<string>();
                    var logLevelJson = jsonObject["@l"]?.GetValue<string>();
                    var messageTemplate = jsonObject["@mt"]?.GetValue<string>();

                    string renderedMessage = TemplateRegex.Replace(messageTemplate, match => FillTemplate(match, jsonObject));

                    string timeFormatted = "HH:mm:ss";
                    if (DateTimeOffset.TryParse(timestamp, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var dtOffset))
                        timeFormatted = dtOffset.ToLocalTime().ToString("HH:mm:ss");

                    string levelAbbreviated = GetLogLevelAbbreviation(logLevelJson);
                    string categoryPlaceholder = jsonObject["@c"]?.GetValue<string>();
                    string messageContent = renderedMessage ?? "";

                    string formattedLogString = $"[{timeFormatted} {levelAbbreviated}] {categoryPlaceholder} {messageContent}";
                    Console.WriteLine(formattedLogString);
                }
            };
        }
        );
    }

    static string FillTemplate(Match match, JsonObject jsonObject)
    {
        try
        {
            string key = match.Groups[1].Value;
            string originalFormat = match.Groups[2].Success ? match.Groups[2].Value : null;

            if (!jsonObject.TryGetPropertyValue(key, out JsonNode valueNode))
                return match.Value; // Key not found, return original placeholder without color

            object actualValue = null;

            if (valueNode != null && valueNode is JsonObject)
                return valueNode.ToString();

            if (valueNode != null && valueNode.GetValue<object>() is JsonElement element)
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        actualValue = element.GetString();
                        break;
                    case JsonValueKind.Number:
                        if (element.TryGetInt32(out int i)) actualValue = i;
                        else if (element.TryGetInt64(out long lng)) actualValue = lng;
                        else if (element.TryGetDouble(out double dbl)) actualValue = dbl;
                        else actualValue = element.ToString();
                        break;
                    case JsonValueKind.True:
                        actualValue = true;
                        break;
                    case JsonValueKind.False:
                        actualValue = false;
                        break;
                    default:
                        actualValue = element.ToString();
                        break;
                }
            else
                actualValue = null;


            string valueColor;
            if (actualValue is string)
                valueColor = AnsiLightGreen;
            else if (actualValue is double || actualValue is float)
                valueColor = AnsiBrightOrange;
            else if (actualValue is int || actualValue is long)
                valueColor = AnsiBrightYellow;
            else if (actualValue is bool)
                valueColor = AnsiBrightCyan;
            else
                valueColor = AnsiBrightRed;

            if (actualValue == null)
                return valueColor + "null" + AnsiReset;

            if (!string.IsNullOrEmpty(originalFormat))
                if (actualValue is IFormattable formattableValue)
                {
                    if (TryFormatValue(formattableValue, originalFormat, out string formattedResult))
                        return valueColor + formattedResult + AnsiReset;
                }
                else if (actualValue is string stringValue)
                {
                    if (TryParseAndFormatNumericString(stringValue, originalFormat, out string numericStringResult))
                        return valueColor + numericStringResult + AnsiReset;
                }

            return valueColor + actualValue.ToString() + AnsiReset;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    static bool TryParseAndFormatNumericString(string stringValue, string format, out string result)
    {
        result = null;
        bool isNumericFormatCandidate = false;
        if (!string.IsNullOrEmpty(format) && "CDEFNPGX".Contains(char.ToUpperInvariant(format[0])))
            isNumericFormatCandidate = true;

        if (isNumericFormatCandidate && double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedDouble))
            return TryFormatValue(parsedDouble, format, out result);

        return false;
    }

    static bool TryFormatValue(IFormattable value, string originalFormat, out string result)
    {
        result = null;
        try
        {
            result = value.ToString(originalFormat, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
        }
        return false;
    }

    static string GetLogLevelAbbreviation(string clefLogLevel)
    {
        string abbreviation = "INF";
        string color = AnsiBrightWhite;

        if (!string.IsNullOrEmpty(clefLogLevel))
            switch (clefLogLevel.ToUpperInvariant())
            {
                case "VERBOSE":
                    abbreviation = "VRB";
                    color = AnsiGrey;
                    break;
                case "DEBUG":
                    abbreviation = "DBG";
                    color = AnsiBrightCyan;
                    break;
                case "INFORMATION":
                    abbreviation = "INF";
                    color = AnsiBrightWhite;
                    break;
                case "WARNING":
                    abbreviation = "WRN";
                    color = AnsiBrightYellow;
                    break;
                case "ERROR":
                    abbreviation = "ERR";
                    color = AnsiBrightRed;
                    break;
                case "FATAL":
                    abbreviation = "FTL";
                    color = AnsiBrightOrange;
                    break;
            }

        return color + abbreviation + AnsiReset;
    }
}
