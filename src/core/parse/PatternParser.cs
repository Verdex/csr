
using csr.core.traverse;

namespace csr.core.parse;

public record ParseResult<T> {
    private ParseResult() { }

    public record Success(T Item) : ParseResult<T>;
    public record Failure(string Message) : ParseResult<T>;
}

public static class PatternParser {
    private static ParseResult<Pattern<T>> Fail<T>(string message) where T : IMatchable<T> => new ParseResult<Pattern<T>>.Failure(message);
    private static ParseResult<Pattern<T>> Success<T>(Pattern<T> item) where T : IMatchable<T> => new ParseResult<Pattern<T>>.Success(item);

    public static ParseResult<Pattern<T>> Parse<T>(string input) where T : IMatchable<T> {
        return Fail<T>("blarg");
    }
}
