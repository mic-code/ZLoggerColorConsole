using NUnit.Framework;
using ZLoggerColorConsole;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ZLoggerColorConsole.Tests;

public class ColorConsoleTests
{
    [Test]
    public void FormatMessage_ShouldHandleParenthesesInKeys()
    {
        var json = new JsonObject
        {
            ["@t"] = "2023-10-27T10:00:00Z",
            ["@c"] = "TestCategory",
            ["@l"] = "Error",
            ["@mt"] = "StackTrace: {new StackTrace(1, true)}",
            ["new StackTrace(1, true)"] = "CorrectValue"
        };

        var jsonString = json.ToJsonString();
        var result = ColorConsole.FormatMessage(jsonString);

        Assert.That(result, Does.Contain("CorrectValue"));
        Assert.That(result, Does.Not.Contain("{new StackTrace(1, true)}"));
    }

    [Test]
    public void FormatMessage_ShouldHandleNestedParenthesesInKeys()
    {
        var json = new JsonObject
        {
            ["@t"] = "2023-10-27T10:00:00Z",
            ["@c"] = "TestCategory",
            ["@l"] = "Error",
            ["@mt"] = "Result: {Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)}",
            ["Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)"] = "Success"
        };

        var jsonString = json.ToJsonString();
        var result = ColorConsole.FormatMessage(jsonString);

        Assert.That(result, Does.Contain("Success"));
        Assert.That(result, Does.Not.Contain("{Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)}"));
    }

    [Test]
    public void FormatMessage_ShouldHandleMultipleComplexKeys()
    {
        var json = new JsonObject
        {
            ["@t"] = "2023-10-27T10:00:00Z",
            ["@c"] = "VulkanBackend.Validation",
            ["@l"] = "Error",
            ["@mt"] = "[{Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)}]{Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage)}",
            ["Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)"] = "VUID-12345",
            ["Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage)"] = "Something went wrong"
        };

        var jsonString = json.ToJsonString();
        var result = ColorConsole.FormatMessage(jsonString);

        Assert.That(result, Does.Contain("VUID-12345"));
        Assert.That(result, Does.Contain("Something went wrong"));
        Assert.That(result, Does.Not.Contain("{Marshal.PtrToStringAnsi"));
    }

    [Test]
    public void FormatMessage_ShouldHandleStandardKeys()
    {
        var json = new JsonObject
        {
            ["@t"] = "2023-10-27T10:00:00Z",
            ["@c"] = "TestCategory",
            ["@l"] = "Information",
            ["@mt"] = "Hello {Name}",
            ["Name"] = "World"
        };

        var jsonString = json.ToJsonString();
        var result = ColorConsole.FormatMessage(jsonString);

        Assert.That(result, Does.Contain("World"));
    }
}
