
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

    // TODO for class: generics, initial constructor, base/interfaces, type constraints,
    //  nested top level, field, property, events, method

    public sealed record Namespace(ImmutableArray<CSharpAst> Contents) : CSharpAst;

    public sealed record ClassDef(Symbol Name) : CSharpAst;



    // TODO: class, record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharp {

    public static IEnumerable<CSharpAst> Parse(string input) {
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetCompilationUnitRoot();
        return root.ChildNodes().SelectMany(Process);
    }

    private static IEnumerable<CSharpAst> Process(SyntaxNode node) {
        switch (node) {
            case ClassDeclarationSyntax c: 
                return [new CSharpAst.ClassDef(new CSharpAst.Symbol(c.Identifier.Text))];
            case BaseNamespaceDeclarationSyntax ns:
                return [new CSharpAst.Namespace(ns.ChildNodes().SelectMany(Process).ToImmutableArray())];
            default:
                return [];
        }
    }
}