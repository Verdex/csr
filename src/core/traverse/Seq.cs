
namespace csr.core.traverse;

public interface ISeqable {
    IEnumerable<ISeqable> Next();
}

public static class Seq {
    public static IEnumerable<ISeqable> ToSeq(this ISeqable target) {
        
    }
}