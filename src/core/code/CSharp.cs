
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

using csr.core.traverse;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csr.core.code;


public abstract record CSharpAst : IMatchable<string, CSharpAst>, ISeqable<CSharpAst> {
    private CSharpAst() { }

    public IEnumerable<CSharpAst> Next() => 
        this switch {
            ReturnType { Contents: var contents } => [contents],
            SuperType { Contents: var contents } => contents,
            GenericTypeDef => [],
            IndexedType { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            SimpleType => [],
            Symbol => [],
            Parameter { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            Namespace { Contents: var contents} => contents,
            ClassDef { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            MethodDef { Name: var name } => [name],
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };

    public CSharpAst Self() => this;

    public void Deconstruct(out string id, out IEnumerable<CSharpAst> contents)
    {
        contents = Next();
        id = this switch {
            ReturnType => "returnType",
            SuperType => "superType",
            GenericTypeDef => "generic",
            IndexedType => "indexedType",
            SimpleType => "simpleType",
            Symbol => "symbol",
            Parameter => "parameter",
            Namespace => "namespace",
            ClassDef => "class",
            MethodDef => "method",
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };
    }

    public sealed record ReturnType(CSharpAst Contents) : CSharpAst;
    public sealed record SuperType(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record GenericTypeDef(string Value) : CSharpAst;
    public sealed record IndexedType(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record SimpleType(string Value) : CSharpAst;
    public sealed record Symbol(string Value) : CSharpAst;

    public sealed record Parameter(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;

    public sealed record Namespace(ImmutableArray<CSharpAst> Contents) : CSharpAst;

    // TODO for class: type constraints,
    //  nested top level, field, property, events, method
    public sealed record ClassDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;

    // TODO for methods:
    // generics, type constraints, parameters, internals, return type
    // TODO do constructors come for free?
    // TODO does short method declaration come for free?
    // TODO what about static void blarg(this T target)

    public sealed record MethodDef(Symbol Name) : CSharpAst;



    // TODO: class, record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharpAstExt {

    private static CSharpAst.Symbol ToSymbol(this string x) => new CSharpAst.Symbol(x);

    public static void Blarg() {

        var input = @"class blargy { 
            public T SomeMethod(T input, object<T> other) where T : IInterface<T> { }
        }";
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetCompilationUnitRoot();

        static void Jabber(SyntaxNode x, int indent) {
            Console.WriteLine($"{new string(' ', indent)}{x.GetText()}:{x.GetType()}");
            foreach(var xx in x.ChildNodes()) {
                Jabber(xx, indent + 1);
            }
        }

        Jabber(root, 0);

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
            case BaseListSyntax x:
                return [new CSharpAst.SuperType(R(x))];
            case SimpleBaseTypeSyntax x:
                return R(x);
            case GenericNameSyntax x: // Note:  This appears to be for indexed types 
                return [new CSharpAst.IndexedType(x.Identifier.Text.ToSymbol(), R(x))];
            case TypeArgumentListSyntax x:
                return R(x);
            case PredefinedTypeSyntax x:
                return [new CSharpAst.SimpleType(x.GetText().ToString())];
            case IdentifierNameSyntax x: // Note:  This appears to be types 
                return [new CSharpAst.SimpleType(x.GetText().ToString())];
            case ParameterSyntax x:
                return [new CSharpAst.Parameter(x.Identifier.Text.ToSymbol(), R(x))];
            case ParameterListSyntax x:
                return R(x);
            case TypeParameterSyntax x:
                return [new CSharpAst.GenericTypeDef(x.GetText().ToString())];
            case TypeParameterListSyntax x:
                return R(x);
            case QualifiedNameSyntax x: 
                return [x.ToFullString().ToSymbol()];
            case MethodDeclarationSyntax x:
            //x.ReturnType
                return [new CSharpAst.MethodDef(x.Identifier.Text.ToSymbol())];
            case ClassDeclarationSyntax x: 
            // TODO:  type constraints
                // Note: TypeParameter[List]Syntax gets generics
                return [new CSharpAst.ClassDef(x.Identifier.Text.ToSymbol(), R(x))];
            case BaseNamespaceDeclarationSyntax x:
                // Note:  QualifiedNameSyntax gets the name out of the namespace object.
                return [new CSharpAst.Namespace(R(x))];
            default:
                return [];
        }
    }
}