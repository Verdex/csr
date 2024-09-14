
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
            GenericTypeDef => [],
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
            GenericTypeDef => "generic",
            Symbol => "symbol",
            ClassDef => "class",
            MethodDef => "method",
            Namespace => "namespace",
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };
    }

    public sealed record GenericTypeDef(string Value) : CSharpAst;
    public sealed record Symbol(string Value) : CSharpAst;

    public sealed record Namespace(ImmutableArray<CSharpAst> Contents) : CSharpAst;

    // TODO for class: initial constructor, base/interfaces, type constraints,
    //  nested top level, field, property, events, method
    public sealed record ClassDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;

    public sealed record MethodDef(Symbol Name) : CSharpAst;



    // TODO: class, record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharpAstExt {

    private static CSharpAst.Symbol ToSymbol(this string x) => new CSharpAst.Symbol(x);

    public static void Blarg() {

        var input = @"class blargy<T, S> { }";
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetCompilationUnitRoot();
        foreach( var x in root.ChildNodes() ) {
            if(x is ClassDeclarationSyntax y) {
                foreach( var yy in y.ChildNodes()) {
                    if (yy is TypeParameterListSyntax z) {
                        foreach( var zz in z.ChildNodes() ) {

                    Console.WriteLine($"{zz.GetText()} : {zz.GetType()}");
                        }
                    }
                    Console.WriteLine($"{yy.GetText()} : {yy.GetType()}");
                }
            }
        }
    }

    public static IEnumerable<CSharpAst> Parse(string input) {
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetCompilationUnitRoot();
        return root.ChildNodes().SelectMany(Process);
    }

    private static IEnumerable<CSharpAst> Process(SyntaxNode node) {
        static ImmutableArray<CSharpAst> R(SyntaxNode n) => n.ChildNodes().SelectMany(Process).ToImmutableArray();
        switch (node) {
            // TODO usings, static using, using as rename
            case TypeParameterSyntax x:
                return [new CSharpAst.GenericTypeDef(node.GetText().ToString())];
            case TypeParameterListSyntax x:
                return R(x);
            case QualifiedNameSyntax x: 
                return [x.ToFullString().ToSymbol()];
            case MethodDeclarationSyntax x:
                return [new CSharpAst.MethodDef(x.Identifier.Text.ToSymbol())];
            case ClassDeclarationSyntax x: 
            // TODO:  base/interfaces, primary constructor, type constraints
                return [new CSharpAst.ClassDef(x.Identifier.Text.ToSymbol(), R(x))];
            case BaseNamespaceDeclarationSyntax x:
                // Note:  QualifiedNameSyntax gets the name out of the namespace object.
                return [new CSharpAst.Namespace(R(x))];
            default:
                return [];
        }
    }
}