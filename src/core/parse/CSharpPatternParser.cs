
using csr.core.traverse;
using csr.core.code;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csr.core.parse;


public abstract record Result {
    private Result() { }

    public sealed record Succ(int Index, Pattern<string, CSharpAst> Pattern) : Result;
    public sealed record Fail(int Index, string Message) : Result;
}

public sealed record PatternParser(Func<string, int, Result> Parser) {
    public Result Parse(string input) => Parser(input, 0);

    public PatternParser Select(Func<Pattern<string, CSharpAst>, Pattern<string, CSharpAst>> map) 
        => new PatternParser((input, index) => Parser(input, index) switch {
            Result.Succ(var newIndex, var pattern) => new Result.Succ(newIndex, map(pattern)),
            var fail => fail,
        });

    public PatternParser SelectMany( Func<Pattern<string, CSharpAst>, PatternParser> next
                                   , Func<Pattern<string, CSharpAst>, Pattern<string, CSharpAst>, Pattern<string, CSharpAst>> final
                                   ) 
        => new PatternParser((input, index) => Parser(input, index) switch {
            Result.Succ(var newIndex, var firstPattern) => next(firstPattern).Parser(input, newIndex) switch {
                Result.Succ(var lastIndex, var lastPattern) => new Result.Succ(lastIndex, final(firstPattern, lastPattern)),
                var fail => fail,
            },
            var fail => fail,
        });

}


public static class CSharpPatternParser {
    /*
    private static ParseResult<Pattern<CSharpAst>> Fail(string message) => new ParseResult<Pattern<CSharpAst>>.Failure(message);
    private static ParseResult<Pattern<CSharpAst>> Success(Pattern<CSharpAst> item) => new ParseResult<Pattern<CSharpAst>>.Success(item);

    public static ParseResult<Pattern<CSharpAst>> Parse(string input) {
        return Fail("blarg");
    }
    */
}

