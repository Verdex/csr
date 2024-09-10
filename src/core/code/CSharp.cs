
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

using csr.core.traverse;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csr.core.code;


public abstract record CSharpAst : IMatchable<string, CSharpAst> {
    private CSharpAst() { }

    public void Deconstruct(out string id, out IEnumerable<CSharpAst> contents)
    {
        throw new NotImplementedException();
    }

    public sealed record Symbol(string Value) : CSharpAst;
    public sealed record SyntaxList(ImmutableList<CSharpAst> Contents) : CSharpAst;

    // TODO for class: generics, initial constructor, base/interfaces, type constraints,
    //  nested top level, field, property, events, method

    public sealed record ClassDef(Symbol Name) : CSharpAst;



    // TODO: class, record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharp {

    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText(@"
            public sealed class X<T>(T x) : Base, Interface where X : class, IThing { 
                public int J { get; }
                public int I;
                public void K() { }

            }");
        var root = tree.GetCompilationUnitRoot();
        foreach(var w in root.ChildNodes()) {

            Console.WriteLine($"{w.GetType()}");

            foreach(var t in w.ChildNodes()) {
                Console.WriteLine($"!!!! {t} :: {t.GetType()}");
            }

            if ( w is ClassDeclarationSyntax h)  {
                foreach( var x in h.Members ) {
                    Console.WriteLine(x);
                }
            }
        }
    }
}