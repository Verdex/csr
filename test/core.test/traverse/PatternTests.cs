
using csr.core.traverse;
using static csr.core.traverse.Pattern;

namespace csr.test.core.traverse;

[TestFixture]
public class PatternTests {

    [Test]
    public void FindWithWild() { 
        var t = Leaf(7);
        var output = t.Find(Wild<Tree>()).Select(x => x.ToList()).ToList();
        Assert.Multiple(() => {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void FindWithExactTemplateVar() {
        var t = Node(Leaf(0), Leaf(0));
        var output = t.Find(Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), TemplateVar<Tree>("a")])).Select(x => x.ToList()).ToList();
        Assert.Multiple(() => {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(1));
            Assert.That(output[0][0].Name, Is.EqualTo("a"));
            Assert.That(output[0][0].Item, Is.EqualTo(Leaf(0)));
        });
    }

    [Test]
    public void FindWithContentsTemplateVar() {
        var t = Node(Leaf(0), Leaf(0));
        var output = t.Find(Contents<Tree>([Capture<Tree>("a"), TemplateVar<Tree>("a")])).Select(x => x.ToList()).ToList();
        Assert.Multiple(() => {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(1));
            Assert.That(output[0][0].Name, Is.EqualTo("a"));
            Assert.That(output[0][0].Item, Is.EqualTo(Leaf(0)));
        });
    }

    [Test]
    public void FindWithKind() {
        var t = Node(Leaf(0), Leaf(0));
        var output = t.Find(Kind<Tree>(typeof(Tree.Node))).Select(x => x.ToList()).ToList();
        Assert.Multiple(() => {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(0));
        });
    }

    // TODO
    // fail template with non existent var name
    // fail template with non matching value

    private static Tree Leaf(byte input) => new Tree.Leaf(input);
    private static Tree Node(Tree left, Tree right) => new Tree.Node(left, right);

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
