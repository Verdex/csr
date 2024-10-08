
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using csr.core.traverse;

namespace csr.core.code;


public abstract record CSharpAst : IMatchable<string, CSharpAst>, ISeqable<CSharpAst> {
    private CSharpAst() { }

    public string SymbolName() => this switch {
        Symbol s => s.Value,
        SimpleType s => s.Value,
        _ => "",
    };

    public IEnumerable<CSharpAst> Next() => 
        this switch {
            Call { Contents: var contents } => contents,
            ClassDef { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            Constraint { Contents: var contents } => contents,
            ConstructorDef { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            FinalizerDef { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            GenericTypeDef => [],
            IndexedType { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            MethodDef { Name: var name, Return: var returnType, Contents: var contents } => new CSharpAst[] {name, returnType}.Concat(contents),
            Namespace { Contents: var contents} => contents,
            Parameter { Name: var name, Contents: var contents } => new [] {name}.Concat(contents),
            ReturnType { Contents: var contents } => contents,
            SimpleType => [],
            SuperType { Contents: var contents } => contents,
            Symbol => [],
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };

    public CSharpAst Self() => this;

    public void Deconstruct(out string id, out IEnumerable<CSharpAst> contents)
    {
        contents = Next();
        id = this switch {
            Call => "call",
            ClassDef => "class",
            Constraint => "constraint",
            ConstructorDef => "constructor",
            FinalizerDef => "finalizer",
            GenericTypeDef => "generic",
            IndexedType => "indexedType",
            MethodDef => "method",
            Namespace => "namespace",
            Parameter => "parameter",
            ReturnType => "returnType",
            SimpleType => "simpleType",
            SuperType => "superType",
            Symbol => "symbol",
            _ => throw new Exception($"Unknown {nameof(CSharpAst)} instance encountered: {this.GetType()}"),
        };
    }

    // TODO for class: 
    //  nested top level, field, property, events, method
    public sealed record Call(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record ClassDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record Constraint(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record ConstructorDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record FinalizerDef(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record GenericTypeDef(string Value) : CSharpAst;
    public sealed record IndexedType(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;

    // TODO for methods:
    //  internals, 

    public sealed record MethodDef(Symbol Name, ReturnType Return, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record Namespace(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record Parameter(Symbol Name, ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record ReturnType(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record SimpleType(string Value) : CSharpAst;
    public sealed record SuperType(ImmutableArray<CSharpAst> Contents) : CSharpAst;
    public sealed record Symbol(string Value) : CSharpAst;





    // TODO: record, (struct record?), struct, enum, interface, namespace, using, delegate

}

public static class CSharpAstExt {

    private static CSharpAst.Symbol ToSymbol(this string x) => new CSharpAst.Symbol(x);

    public static void Blarg() {

        var input = @"class blargy<T> where T : IInterface { 
            public int Blarg(this int blarg) => 0;

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
            case InvocationExpressionSyntax x:
                return [new CSharpAst.Call(R(x))];
            case ExpressionStatementSyntax x:
                return R(x);
            case EqualsValueClauseSyntax x:
                return R(x);
            case LocalDeclarationStatementSyntax x:
                return R(x);
            case VariableDeclarationSyntax x:
                return R(x);
            case StatementSyntax x: 
                return R(x);
            case TypeParameterConstraintSyntax x : // Note:  Supposedly covers:  TypeConstraintSyntax, ConstructorConstraintSyntax, and ClassOrStructConstraintSyntax
                return R(x);
            case TypeParameterConstraintClauseSyntax x:
                return [new CSharpAst.Constraint(R(x))];
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
            case IdentifierNameSyntax x: 
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
                return [new CSharpAst.MethodDef(x.Identifier.Text.ToSymbol(), new CSharpAst.ReturnType(R(x.ReturnType)), R(x))];
            case ConstructorDeclarationSyntax x: 
                return [new CSharpAst.ConstructorDef(x.Identifier.Text.ToSymbol(), R(x))];
            case DestructorDeclarationSyntax x:
                return [new CSharpAst.FinalizerDef(x.Identifier.Text.ToSymbol(), R(x))];
            case ClassDeclarationSyntax x: 
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