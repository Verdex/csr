
using csr.core.traverse;
using csr.core.code;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Security.Cryptography;

namespace csr.core.parse;


public abstract record Result<T> {
    private Result() { }

    public sealed record Succ(int Index, T Item) : Result<T>;
    public sealed record Fail(int Index, string Message) : Result<T>;
}

public sealed record Parser<T>(Func<string, int, Result<T>> P) {
    public Result<T> Parse(string input) => P(input, 0);

    public Parser<S> Select<S>(Func<T, S> map) 
        => new Parser<S>((input, index) => P(input, index) switch {
            Result<T>.Succ(var newIndex, var pattern) => new Result<S>.Succ(newIndex, map(pattern)),
            Result<T>.Fail(var newIndex, var message) => new Result<S>.Fail(newIndex, message),
            _ => throw new Exception("Encountered unsupported Result"),
        });

    public Parser<F> SelectMany<S, F>( Func<T, Parser<S>> next, Func<T, S, F> final ) 
        => new Parser<F>((input, index) => P(input, index) switch {
            Result<T>.Succ(var newIndex, var firstPattern) => next(firstPattern).P(input, newIndex) switch {
                Result<S>.Succ(var lastIndex, var lastPattern) => new Result<F>.Succ(lastIndex, final(firstPattern, lastPattern)),
                Result<S>.Fail(var lastIndex, var message) => new Result<F>.Fail(lastIndex, message),
                _ => throw new Exception("Encountered unsupported Result"),
            },
            Result<T>.Fail(var newIndex, var message) => new Result<F>.Fail(newIndex, message),
            _ => throw new Exception("Encountered unsupported Result"),
        });

}


public static class CSharpPatternParser {
    public static void Blah() {
        var blah = from x in new Parser<Pattern<string, CSharpAst>>((a, b) => new Result<Pattern<string, CSharpAst>>.Succ(0, null))
                   from w in new Parser<Pattern<string, CSharpAst>>((a, b) => new Result<Pattern<string, CSharpAst>>.Succ(0, null))
                   from z in new Parser<Pattern<string, CSharpAst>>((a, b) => new Result<Pattern<string, CSharpAst>>.Succ(0, null))
                   select new Pattern<string, CSharpAst>.Exact("id", [x, w, z]);
    }
    /*
    private static ParseResult<Pattern<CSharpAst>> Fail(string message) => new ParseResult<Pattern<CSharpAst>>.Failure(message);
    private static ParseResult<Pattern<CSharpAst>> Success(Pattern<CSharpAst> item) => new ParseResult<Pattern<CSharpAst>>.Success(item);

    public static ParseResult<Pattern<CSharpAst>> Parse(string input) {
        return Fail("blarg");
    }
    */
}

