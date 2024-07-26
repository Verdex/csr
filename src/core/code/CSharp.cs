
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
            case CSharpAst.CSType.SimpleType:
                id = typeof(CSharpAst.CSType.SimpleType);
                contents = [];
                break;
            case CSharpAst.CSType.IndexedType x:
                id = typeof(CSharpAst.CSType.IndexedType);
                contents = x.Types;
                break;
            case CSharpAst.SuperTypeList x:
                id = typeof(CSharpAst.SuperTypeList);
                contents = x.SuperTypes;
                break;
            case CSharpAst.Class(var name, var superTypes, var members):
                id = typeof(CSharpAst.Class);
                contents = [name, superTypes, members]; 
                break;
            default:
                throw new NotImplementedException($"Unknown {nameof(CSharpAst)} case {this.GetType()}");
        }
    }
    
    public record Symbol(string Value) : CSharpAst;

    public abstract record CSType : CSharpAst {
        private CSType() { }

        public record SimpleType(string Name) : CSType;
        public record IndexedType(string Name, ImmutableList<CSType> Types) : CSType;
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