
using System.Collections;

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
    public record MatchWith(Func<IDictionary<string, T>, Pattern<T>> Func) : Pattern<T>;
}

public static class Pattern {
    public static Pattern<T> Wild<T>() where T : IMatchable<T> => new Pattern<T>.Wild();

    public static Pattern<T> Capture<T>(string name) where T : IMatchable<T> => new Pattern<T>.Capture(name);
    public static Pattern<T> TemplateVar<T>(string name) where T : IMatchable<T> => new Pattern<T>.TemplateVar(name);

    public static Pattern<T> Exact<T>(Type id, IEnumerable<Pattern<T>> contents) where T : IMatchable<T> => new Pattern<T>.Exact(id, contents);
    public static Pattern<T> Contents<T>(IEnumerable<Pattern<T>> contents) where T : IMatchable<T> => new Pattern<T>.Contents(contents);

    public static Pattern<T> And<T>(Pattern<T> left, Pattern<T> right) where T : IMatchable<T> => new Pattern<T>.And(left, right);
    public static Pattern<T> Or<T>(Pattern<T> left, Pattern<T> right) where T : IMatchable<T> => new Pattern<T>.Or(left, right);


    public static Pattern<T> PathNext<T>() where T : IMatchable<T> => new Pattern<T>.PathNext();
    public static Pattern<T> Path<T>(IEnumerable<Pattern<T>> patterns) where T : IMatchable<T> => new Pattern<T>.Path(patterns);
    public static Pattern<T> SubContentPath<T>(IEnumerable<Pattern<T>> patterns) where T : IMatchable<T> => new Pattern<T>.SubContentPath(patterns);

    public static Pattern<T> Predicate<T>(Func<T, bool> predicate) where T : IMatchable<T> => new Pattern<T>.Predicate(predicate);
    public static Pattern<T> MatchWith<T>(Func<IDictionary<string, T>, Pattern<T>> func) where T : IMatchable<T> => new Pattern<T>.MatchWith(func);

    public static IEnumerable<IEnumerable<(string Name, T Item)>> Find<T>(this T data, Pattern<T> pattern) where T : IMatchable<T> =>
        new PatternEnumerable<T>(data, pattern);

    public static IEnumerable<IDictionary<string, T>> FindDict<T>(this T data, Pattern<T> pattern) where T : IMatchable<T> =>
        data.Find(pattern).Select(x => x.ToDictionary(k => k.Name, v => v.Item));

    public class PatternEnumerable<T>(T data, Pattern<T> pattern) : IEnumerable<IEnumerable<(string Name, T Item)>> where T : IMatchable<T> {
        private PatternEnumerator<T> Enumerator() => new PatternEnumerator<T>(data, pattern);
        public IEnumerator<IEnumerable<(string Name, T Item)>> GetEnumerator() => Enumerator();
        IEnumerator IEnumerable.GetEnumerator() => Enumerator();
    }

    public class PatternEnumerator<T> : IEnumerator<IEnumerable<(string Name, T Item)>> where T : IMatchable<T> {

        private readonly T _data;
        private readonly Pattern<T> _pattern;

        private Stack<(T, Pattern<T>)> _work = new();
        private List<(string, T)> _captures = new();
        private List<T> _nexts = new();
        private Stack<(List<(string, T)> Captures, Stack<(T, Pattern<T>)> Work, List<T> Nexts)> _alternatives = new();

        public PatternEnumerator(T data, Pattern<T> pattern) {
            _work.Push((data, pattern));
            _data = data;
            _pattern = pattern;
        }

        public IEnumerable<(string Name, T Item)> Current => _captures;

        public bool MoveNext() {
            if (_work.Count == 0 && _alternatives.Count == 0) {
                return false;
            }

            if (_work.Count == 0) {
                SwitchToAlternative();
            }

            while (_work.Count != 0) {
                var (data, pattern) = _work.Pop();
                switch (pattern) {
                    case Pattern<T>.Wild: break;

                    case Pattern<T>.Capture c:
                        _captures.Add((c.Name, data));
                        break;

                    default:
                        throw new NotImplementedException("TODO");
                }
            }

            return true;
        }

        object IEnumerator.Current => _captures;

        public void Reset() {
            _captures = new();
            _nexts = new();
            _alternatives = new();
            _work = new();
            _work.Push((_data, _pattern));
        }

        public void Dispose() { }

        private void SwitchToAlternative() {
            var alt = _alternatives.Pop();
            _work = alt.Work;
            _captures = alt.Captures;
            _nexts = alt.Nexts;
        }
    }
}
