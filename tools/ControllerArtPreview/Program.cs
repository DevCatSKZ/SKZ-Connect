using System.Reflection;

// Nur Zeichenflächen: keine Form, kein Application.Run, keine Geräteverbindung.
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var assembly = Assembly.Load("SKZ-Connect");
        var type = args.Contains("--themes") ? "ThemePreview" : args.Contains("--icons") ? "ControllerIconExporter" : "RenderCheck";
        assembly.GetType("Switch2Pro.Bridge." + type, throwOnError: true)!
            .GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, [Path.GetFullPath(args[0])]);
    }
}
