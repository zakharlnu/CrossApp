using Core;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

var report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    Console.WriteLine(JsonSerializer.Serialize(report, options));
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС: {report.OsDescription}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура: {report.ProcessArchitecture}");
    Console.WriteLine($"RID визначено: {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог: {report.BaseDirectory}");
    Console.WriteLine($"Примітка: {report.BuildNote}");
}