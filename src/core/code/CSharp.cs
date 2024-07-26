
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace csr.core.code;

public abstract record CSharpAst {
    private CSharpAstS() { }


}

public static class CSharp {
    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText("class X { }");
        var root = tree.GetCompilationUnitRoot();
    }
}