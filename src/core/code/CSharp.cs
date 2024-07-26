
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

using csr.core.traverse;

namespace csr.core.code;

public abstract record CSharpAst : IMatchable<CSharpAst> {
    private CSharpAst() { }

    public void Deconstruct(out Type id, out IEnumerable<CSharpAst> contents) {
        switch (this) {
            case CSharpAst.Symbol:
                id = typeof(CSharpAst.Symbol);
                contents = [];
                break;
            case CSharpAst.Class x:
                id = typeof(CSharpAst.Class);
                contents = [x.Name]; // TODO
                break;
            default:
                throw new NotImplementedException($"Unknown {nameof(CSharpAst)} case {this.GetType()}");
        }
    }
    
    public record Symbol(string Value) : CSharpAst;
    // TODO super type list
    // Symbol<Indexed>
    public record Class(Symbol Name, ImmutableList<Symbol> SuperTypes, ImmutableList<CSharpAst> Contents) : CSharpAst;

}

public static class CSharp {
    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText("class X { }");
        var root = tree.GetCompilationUnitRoot();
    }
}