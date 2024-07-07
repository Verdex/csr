
using csr.core.traverse;
using static csr.core.traverse.Pattern;

namespace csr.test.core.traverse;

[TestFixture]
public class PatternTests {

    [Test]
    public void FindWithWild() { 
        var t = new Tree.Leaf(7);
        var output = t.Find(Wild<Tree>());
        // TODO 
    }

    private abstract record Tree : IMatchable<Tree> {
        private Tree() { }

        public record Leaf(byte Value) : Tree;
        public record Node(Tree Left, Tree Right) : Tree;

        public void Deconstruct(out Type id, out IEnumerable<Tree> contents) {
            switch (this) {
                case Leaf l:
                    id = typeof(Leaf);
                    contents = [];
                    break;
                case Node n:
                    id = typeof(Node);
                    contents = [n.Left, n.Right];
                    break;
                default:
                    throw new NotImplementedException($"Unknown {nameof(Tree)} case {this.GetType()}");
            }
        }
    }
}
