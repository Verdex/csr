
using csr.core.traverse;
using csr.core.code;

namespace csr.core.parse;


public abstract record ParseResult<T> {
    private ParseResult() { }

    public record Success(T Item) : ParseResult<T>;
    public record Failure(string Message) : ParseResult<T>;
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

