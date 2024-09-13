
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
            Symbol => [],
            ClassDef { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            MethodDef => [],
            Namespace { Contents: var contents} => contents,
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };

    public CSharpAst Self() => this;

    public void Deconstruct(out string id, out IEnumerable<CSharpAst> contents)
    {
        contents = Next();
        id = this switch {
            Symbol => "symbol",
            ClassDef => "class",
            MethodDef => "method",
            Namespace => "namespace",
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };
    }

    public sealed record Symbol(string Value) : CSharpAst;

    public sealed record Namespace(ImmutableArray<CSharpAst> Contents) : CSharpAst;

    // TODO for class: generics, initial constructor, base/interfaces, type constraints,
    //  nested top level, field, property, events, method
    public sealed record ClassDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;

    public sealed record MethodDef() : CSharpAst;



    // TODO: class, record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharp {

    public static IEnumerable<CSharpAst> Parse(string input) {
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetCompilationUnitRoot();
        return root.ChildNodes().SelectMany(Process);
    }

    private static IEnumerable<CSharpAst> Process(SyntaxNode node) {
        static ImmutableArray<CSharpAst> R(SyntaxNode n) => n.ChildNodes().SelectMany(Process).ToImmutableArray();
        switch (node) {
            case QualifiedNameSyntax qns: 
                return [new CSharpAst.Symbol(qns.ToFullString())];
            case MethodDeclarationSyntax m:
                return [new CSharpAst.MethodDef()];
            case ClassDeclarationSyntax c: 
                return [new CSharpAst.ClassDef(new CSharpAst.Symbol(c.Identifier.Text), R(c))];
            case BaseNamespaceDeclarationSyntax ns:
                return [new CSharpAst.Namespace(R(ns))];
            default:
                return [];
        }
    }
}