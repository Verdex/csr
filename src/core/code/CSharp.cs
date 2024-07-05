
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace csr.core.code;

public static class CSharp {
    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText("class X { }");
        var root = tree.GetCompilationUnitRoot();
    }
}