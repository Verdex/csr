
using csr.core.code;
using csr.core.traverse;

namespace csr;

public static class Program {

        class blargy : List<Task> { }

    public static void Main() {
        var currentDirectory = Directory.GetCurrentDirectory();

        //CSharpAstExt.Blarg();

        //*
        var csFiles = Directory.GetDirectories(currentDirectory, "", SearchOption.AllDirectories)
            .Where(d => new [] {"Debug", "Release", "bin", "obj", ".git"}.All(target => !IgnoreDir(d, target)))
            .SelectMany(d => Directory.GetFiles(d))
            .Where(f => f.EndsWith(".cs"))
            .Select(File.ReadAllText)
            .SelectMany(CSharpAstExt.Parse);


        foreach(var ww in csFiles) {
            foreach( var x in ww.ToSeq()) {
                Console.WriteLine(x);
            }
            Console.WriteLine("=");
        }
        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();
            Console.WriteLine($"{line}");
        }
        //*/
    }

    private static bool IgnoreDir(string dir, string endsWith) 
        => dir.EndsWith($"{Path.DirectorySeparatorChar}{endsWith}") 
        || dir.Contains($"{Path.DirectorySeparatorChar}{endsWith}{Path.DirectorySeparatorChar}");

}