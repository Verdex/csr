
namespace csr.core.traverse;

public interface IMatchable<T> {
    void Deconstruct(out Type id, out IEnumerable<T> contents);
}

public abstract record Pattern<T> where T : IMatchable<T> {
    private Pattern() { }

    public record Wild : Pattern<T>;

    public record Capture(string Name) : Pattern<T>;
    public record TemplateVar(string Name) : Pattern<T>;

    public record Exact(Type Id, IEnumerable<Pattern<T>> Cs) : Pattern<T>;
    public record Contents(IEnumerable<Pattern<T>> Cs) : Pattern<T>;

    public record And(Pattern<T> Left, Pattern<T> Right) : Pattern<T>;
    public record Or(Pattern<T> Left, Pattern<T> Right) : Pattern<T>;

    public record PathNext : Pattern<T>;
    public record Path(IEnumerable<Pattern<T>> Ps) : Pattern<T>;
    public record SubContentPath(IEnumerable<Pattern<T>> Ps) : Pattern<T>;

    public record Predicate(Func<T, bool> Pred) : Pattern<T>;
    public record MatchWith(Func<IDictionary<string, T>, Pattern<T>> With) : Pattern<T>;
}

public static class Pattern {
    public static Pattern<T> Wild<T>() where T : IMatchable<T> => new Pattern<T>.Wild();

    public static IEnumerable<(string Name, T Item)> Find<T>(this IMatchable<T> data, Pattern<T> pattern) where T : IMatchable<T> {
        throw new NotImplementedException();
    }

    public static IDictionary<string, T> FindDict<T>(this IMatchable<T> data, Pattern<T> pattern) where T : IMatchable<T> =>
        data.Find(pattern).ToDictionary(k => k.Name, v => v.Item);
}
