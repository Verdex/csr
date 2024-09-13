
using csr.core.code;

namespace csr;

public static class Program {

    public static void Main() {
        var currentDirectory = Directory.GetCurrentDirectory();

        var csFiles = Directory.GetDirectories(currentDirectory, "", SearchOption.AllDirectories)
            .Where(d => new [] {"Debug", "Release", "bin", "obj", ".git"}.All(target => !IgnoreDir(d, target)))
            .SelectMany(d => Directory.GetFiles(d))
            .Where(f => f.EndsWith(".cs"));

        var x = File.ReadAllText(csFiles.First());

        var w = CSharp.Parse(x).ToList();
        foreach(var ww in w) {
            Console.WriteLine(ww);
        }
        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();
            Console.WriteLine($"{line}");
        }
    }

    private static bool IgnoreDir(string dir, string endsWith) 
        => dir.EndsWith($"{Path.DirectorySeparatorChar}{endsWith}") 
        || dir.Contains($"{Path.DirectorySeparatorChar}{endsWith}{Path.DirectorySeparatorChar}");

}