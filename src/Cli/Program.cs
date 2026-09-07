using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

var info = new 
{
    Title = "CrossApp – практикум з крос-платформного програмування",
    StudentData = "Студент: Ільчук Захар, група ФЕІ-32",
    OSDescription = RuntimeInformation.OSDescription,
    EnvironmentOSVersion = Environment.OSVersion.VersionString,
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture,
    DotNetVersion = Environment.Version,
    FrameworkDescription = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Предметна область: Замовлення (клієнти, товари, замовлення)"

};

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    Console.WriteLine(info.Title);
    Console.WriteLine(info.StudentData);
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription) : {info.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {info.EnvironmentOSVersion}");
    Console.WriteLine($"Архітектура процесу : {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {info.DotNetVersion}");
    Console.WriteLine($"Runtime : {info.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку : {info.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {info.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine(info.Domain);
}