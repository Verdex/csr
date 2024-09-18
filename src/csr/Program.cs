
using csr.core.code;
using csr.core.traverse;

namespace csr;

public static class Program {

        class blargy : List<Task> { 

            void Method1( object x ) { }
            T Method2<T>( object x ) { return default; }
            IList<T> Method3<T>( object x ) { return default; }

        }

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