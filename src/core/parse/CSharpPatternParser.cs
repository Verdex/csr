
using csr.core.traverse;
using csr.core.code;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csr.core.parse;



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

