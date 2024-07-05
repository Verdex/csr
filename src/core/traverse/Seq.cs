
namespace csr.core.traverse;

public interface ISeqable {
    IEnumerable<ISeqable> Next();
}

public static class Seq {
    public static IEnumerable<ISeqable> ToSeq(this ISeqable target) {
        var s = new List<ISeqable> { target };
        while(s.Count > 0) {
            var t = s[^1];
            var ns = t.Next();
            s.AddRange(ns);
            yield return t;
        }
    }
}