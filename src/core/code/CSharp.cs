
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

using csr.core.traverse;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csr.core.code;


public abstract record CSharpAst : IMatchable<string, CSharpAst> {
    private CSharpAst() { }

}

    public record SuperTypeList(ImmutableList<CSType> SuperTypes) : CSharpAst;
    
    public abstract record ClassMember : CSharpAst {
        private ClassMember() { }

        public record Field(string Name, CSType type) : ClassMember; // TODO initial value (optional), accessibility, readonly, static, etc?
    }

    public record ClassMemberList(ImmutableList<ClassMember> Members) : CSharpAst;

    public record Class(Symbol Name, SuperTypeList SuperTypes, ClassMemberList Members) : CSharpAst;

}

public static class CSharp {
    public static void Parse() {
        var tree = CSharpSyntaxTree.ParseText("class X { }");
        var root = tree.GetCompilationUnitRoot();
    }
}