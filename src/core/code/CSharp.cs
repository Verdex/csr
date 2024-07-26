
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using csr.core.traverse;

namespace csr.core.code;

public abstract record CSharpAst : IMatchable<CSharpAst> {
    private CSharpAst() { }

    public void Deconstruct(out Type id, out IEnumerable<CSharpAst> contents) {
        switch (this) {
            case CSharpAst.Class:
                id = typeof(CSharpAst.Class);
                contents = [];
                break;
            default:
                throw new NotImplementedException($"Unknown {nameof(CSharpAst)} case {this.GetType()}");
        }
    }
    

    public record Class() : CSharpAst {

    }

}

public static class CSharp {
    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText("class X { }");
        var root = tree.GetCompilationUnitRoot();
    }
}