
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

    public record Exact(Type Id, List<Pattern<T>> Cs) : Pattern<T>;
    public record Contents(List<Pattern<T>> Cs) : Pattern<T>;
    public record Kind(Type Id) : Pattern<T>;

    public record And(Pattern<T> Left, Pattern<T> Right) : Pattern<T>;
    public record Or(Pattern<T> Left, Pattern<T> Right) : Pattern<T>;

    public record PathNext : Pattern<T>;
    public record Path(List<Pattern<T>> Ps) : Pattern<T>;
    public record SubContentPath(List<Pattern<T>> Ps) : Pattern<T>;

    public record Predicate(Func<T, bool> Pred) : Pattern<T>;
    public record MatchWith(Func<IReadOnlyDictionary<string, T>, Pattern<T>> Func) : Pattern<T>;
}

public static class Pattern {
    public static Pattern<T> Wild<T>() where T : IMatchable<T> => new Pattern<T>.Wild();

    public static Pattern<T> Capture<T>(string name) where T : IMatchable<T> => new Pattern<T>.Capture(name);
    public static Pattern<T> TemplateVar<T>(string name) where T : IMatchable<T> => new Pattern<T>.TemplateVar(name);

    public static Pattern<T> Exact<T>(Type id, IEnumerable<Pattern<T>> contents) where T : IMatchable<T> => new Pattern<T>.Exact(id, contents.ToList());
    public static Pattern<T> Contents<T>(IEnumerable<Pattern<T>> contents) where T : IMatchable<T> => new Pattern<T>.Contents(contents.ToList());
    public static Pattern<T> Kind<T>(Type id) where T : IMatchable<T> => new Pattern<T>.Kind(id);

    public static Pattern<T> And<T>(Pattern<T> left, Pattern<T> right) where T : IMatchable<T> => new Pattern<T>.And(left, right);
    public static Pattern<T> Or<T>(Pattern<T> left, Pattern<T> right) where T : IMatchable<T> => new Pattern<T>.Or(left, right);

    public static Pattern<T> PathNext<T>() where T : IMatchable<T> => new Pattern<T>.PathNext();
    public static Pattern<T> Path<T>(IEnumerable<Pattern<T>> patterns) where T : IMatchable<T> => new Pattern<T>.Path(patterns.ToList());
    public static Pattern<T> SubContentPath<T>(IEnumerable<Pattern<T>> patterns) where T : IMatchable<T> => new Pattern<T>.SubContentPath(patterns.ToList());

    public static Pattern<T> Predicate<T>(Func<T, bool> predicate) where T : IMatchable<T> => new Pattern<T>.Predicate(predicate);
    public static Pattern<T> MatchWith<T>(Func<IReadOnlyDictionary<string, T>, Pattern<T>> func) where T : IMatchable<T> => new Pattern<T>.MatchWith(func);

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

                    case Pattern<T>.TemplateVar t: {
                        var (_, item) = _captures.Find(c => c.Item1.Equals(t.Name));
                        if (item is null || !item.Equals(data)){
                            // Note:  If the item is null then we've been given an non-existent variable name.
                            // Note:  Switching to alternative on failure.
                            if (_alternatives.Count > 0) {
                                SwitchToAlternative();
                            }
                            else {
                                return false;
                            }
                        }
                        break;
                    }

                    case Pattern<T>.Exact e: {
                        var (id, cs) = data;
                        var dataContents = cs.ToList();
                        if (id == e.Id && e.Cs.Count == dataContents.Count) {
                            foreach( var w in dataContents.Zip(e.Cs).Reverse() ) {
                                _work.Push(w);
                            }
                        }
                        else if(_alternatives.Count > 0) {
                            SwitchToAlternative();
                        }
                        else { 
                            return false;
                        }
                        break;
                    }

                    case Pattern<T>.Contents c: {
                        var (_, cs) = data;
                        var dataContents = cs.ToList();
                        if (dataContents.Count == c.Cs.Count) {
                            foreach( var w in cs.Zip(c.Cs).Reverse() ) {
                                _work.Push(w);
                            }
                        }
                        else if(_alternatives.Count > 0) {
                            SwitchToAlternative();
                        }
                        else { 
                            return false;
                        }
                        break;
                    }

                    case Pattern<T>.Kind k: {
                        var (id, _) = data;
                        if (id != k.Id) {
                            if (_alternatives.Count > 0) {
                                SwitchToAlternative();
                            }
                            else {
                                return false;
                            }
                        }
                        break;
                    }

                    case Pattern<T>.And a:
                        _work.Push((data, a.Right));
                        _work.Push((data, a.Left));
                        break;

                    case Pattern<T>.Or o: {
                        var w = Dup(_work);
                        w.Push((data, o.Right));
                        AddAlternative(w);
                        _work.Push((data, o.Left));
                        break;
                    }

                    case Pattern<T>.PathNext:
                        _nexts.Add(data);
                        break;

                    case Pattern<T>.Path(var ps) when ps.Count == 0: break;
                    case Pattern<T>.Path(var ps): {
                        // Note:  Current work cloned off for alternatives
                        var altWork = Dup(_work);
                        var e = (PatternEnumerator<T>)data.Find(ps[0]).GetEnumerator();

                        // Note:  Inject existing _captures into e's _captures so that
                        // template variables work inside of the inner Find.
                        e._captures.AddRange(_captures);

                        // Note: A failure to move next means that the entire Path has failed
                        if (!e.MoveNext()) { 
                            if (_alternatives.Count > 0) {
                                SwitchToAlternative();
                                // Note:  SwitchToAlternative has to be the last thing done in a case!
                                break;
                            }
                            else {
                                return false;
                            }
                        }

                        // Note:  e.Current contains all of the existing _captures, so to
                        // avoid duplicate captures assign the Current instead of appending it
                        _captures = e.Current.ToList();

                        if (e._nexts.Count > 0) {
                            var nextPathData = e._nexts[0];
                            var nextPathPattern = Path<T>(ps[1..]);

                            foreach( var next in e._nexts[1..] ) {
                                var w = Dup(_work);
                                w.Push((next, nextPathPattern));
                                AddAlternative(w, nexts: []);
                            }

                            _work.Push((nextPathData, nextPathPattern));
                        }

                        // Note:  For each alternative of e, stuff all of them into alternatives
                        while (e.MoveNext()) {
                            var captures = e.Current.ToList();

                            if (e._nexts.Count > 0) {
                                var nextPathPattern = Path<T>(ps[1..]);

                                foreach( var next in e._nexts ) {
                                    var w = Dup(altWork);
                                    w.Push((next, nextPathPattern));
                                    AddAlternative(w, captures: captures, nexts: []);
                                }
                            }
                            else {
                                AddAlternative(Dup(altWork), captures: captures, nexts: []);
                            }

                        }
                        
                        break;
                    }

                    case Pattern<T>.SubContentPath(var ps): {
                        var (_, cs) = data;
                        var dataContents = cs.ToList();
                        if (ps.Count <= dataContents.Count) {
                            foreach( var index in Enumerable.Range(1, (dataContents.Count - ps.Count)).Reverse() ) { 
                                var w = Dup(_work);

                                var targetData = dataContents[index..(index + ps.Count)]; 

                                foreach( var x in targetData.Zip(ps).Reverse() ) {
                                    w.Push(x);
                                }

                                AddAlternative(w);
                            }

                            {
                                var targetData = dataContents[0..ps.Count];
                                foreach( var x in targetData.Zip(ps).Reverse() ) {
                                    _work.Push(x);
                                }
                            }
                        }
                        else {
                            if (_alternatives.Count > 0) {
                                SwitchToAlternative();
                            }
                            else {
                                return false;
                            }
                        }

                        break;
                    }

                    case Pattern<T>.Predicate(var p): {
                        if (!p(data)) {
                            if (_alternatives.Count > 0) {
                                SwitchToAlternative();
                            }
                            else {
                                return false;
                            }
                        }
                        break;
                    }

                    case Pattern<T>.MatchWith(var f): {
                        var p = f(_captures.ToDictionary(c => c.Item1, c => c.Item2));
                        _work.Push((data, p));
                        break;
                    }

                    default:
                        if (_alternatives.Count > 0) {
                            SwitchToAlternative();
                        }
                        else {
                            return false;
                        }
                        break;
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

        private void AddAlternative(Stack<(T, Pattern<T>)> work, List<(String, T)>? captures = null, List<T>? nexts = null) {
            var c = captures ?? Dup(_captures);
            var n = nexts ?? Dup(_nexts);
            _alternatives.Push((c, work, n));
        }

        private void SwitchToAlternative() {
            var alt = _alternatives.Pop();
            _work = alt.Work;
            _captures = alt.Captures;
            _nexts = alt.Nexts;
        }

        private static Stack<X> Dup<X>(Stack<X> s) => new (s);
        private static List<X> Dup<X>(List<X> l) => new (l);
    }
}
