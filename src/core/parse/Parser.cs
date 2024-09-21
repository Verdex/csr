
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

    // TODO alternate
    // TODO where?
    // TODO end?
    // TODO zero or more?
    // TODO maybe ?

}
