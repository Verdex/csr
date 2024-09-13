
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

using csr.core.traverse;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography;

namespace csr.core.code;


public abstract record CSharpAst : IMatchable<string, CSharpAst>, ISeqable<CSharpAst> {
    private CSharpAst() { }

    public IEnumerable<CSharpAst> Next() => 
        this switch {
            CSharpAst.Symbol => [],
            CSharpAst.ClassDef { Name: var name } => [name],
            CSharpAst.Namespace { Contents: var contents} => contents,
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };

    public CSharpAst Self() => this;

    public void Deconstruct(out string id, out IEnumerable<CSharpAst> contents)
    {
        contents = Next();
        id = this switch {
            CSharpAst.Symbol => "symbol",
            CSharpAst.ClassDef => "class",
            CSharpAst.Namespace => "namespace",
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };
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