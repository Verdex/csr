
using System.Collections.Immutable;

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
            Result<T>.Fail(var failIndex, var message) => new Result<S>.Fail(failIndex, message),
            _ => throw new Exception("Encountered unsupported Result in Select"),
        });

    public Parser<F> SelectMany<S, F>( Func<T, Parser<S>> next, Func<T, S, F> final ) 
        => new Parser<F>((input, index) => P(input, index) switch {
            Result<T>.Succ(var newIndex, var firstPattern) => next(firstPattern).P(input, newIndex) switch {
                Result<S>.Succ(var lastIndex, var lastPattern) => new Result<F>.Succ(lastIndex, final(firstPattern, lastPattern)),
                Result<S>.Fail(var failIndex, var message) => new Result<F>.Fail(failIndex, message),
                _ => throw new Exception("Encountered unsupported Result in SelectMany(a)"),
            },
            Result<T>.Fail(var failIndex, var message) => new Result<F>.Fail(failIndex, message),
            _ => throw new Exception("Encountered unsupported Result in SelectMany(b)"),
        });

    public Parser<T> Where(Func<T, bool> pred) 
        => new Parser<T>((input, index) => P(input, index) switch {
            Result<T>.Succ(var newIndex, var pattern) when pred(pattern) => new Result<T>.Succ(newIndex, pattern),
            Result<T>.Succ(var failIndex, _) => new Result<T>.Fail(failIndex, "parse successful; predicate failed"),
            Result<T>.Fail(var failIndex, var message) => new Result<T>.Fail(failIndex, $"parse failed in Where with {message}"),
            _ => throw new Exception("Encountered unsupported Result in Where"),
        });

    public Parser<T> Or(params Parser<T>[] other) => new Parser<T>((input, index) => {
        {
            var r = P(input, index);
            if(r is Result<T>.Succ) {
                return r;
            }
        }

        foreach(var p in other) {
            var r = P(input, index);
            if(r is Result<T>.Succ) {
                return r;
            }
        }

        return new Result<T>.Fail(index, "Or failure");
    });

    // TODO where?
    // TODO end?
    // TODO zero or more?
    // TODO maybe ?

}
