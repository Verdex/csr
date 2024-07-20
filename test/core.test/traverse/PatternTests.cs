
using csr.core.traverse;
using static csr.core.traverse.Pattern;

namespace csr.test.core.traverse;

[TestFixture]
public class PatternTests {

    private static List<List<(string Name, Tree Item)>> F(Tree d, Pattern<Tree> p) => d.Find<Tree>(p).Select(x => x.ToList()).ToList();
    private static void A(List<List<(string Name, Tree Item)>> output, List<List<(string Name, Tree Item)>> expected) {
        Assert.Multiple(() => {
            Assert.That(output.Count, Is.EqualTo(expected.Count));
            foreach( var (o, e, i) in output.Zip(expected).Select((x, index) => (x.Item1, x.Item2, index) )) {
                Assert.That(o.Count, Is.EqualTo(e.Count), $"Index {i}");
                foreach( var (olet, elet) in o.Zip(e) ) {
                    Assert.That(olet.Name, Is.EqualTo(elet.Name), $"Index {i}");
                    Assert.That(olet.Item, Is.EqualTo(elet.Item), $"Index {i}");
                }
            }
        });
    }

    [Test]
    public void FindWithWild() { 
        var t = Leaf(7);
        var output = F(t, Wild<Tree>());
        A(output, [[]]);
    }

    [Test]
    public void FindWithExactTemplateVar() {
        var t = Node(Leaf(0), Leaf(0));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), TemplateVar<Tree>("a")]));
        A(output, [[("a", Leaf(0))]]);
    }

    [Test]
    public void FindWithContentsTemplateVar() {
        var t = Node(Leaf(0), Leaf(0));
        var output = F(t, Contents<Tree>([Capture<Tree>("a"), TemplateVar<Tree>("a")]));
        A(output, [[("a", Leaf(0))]]);
    }

    [Test]
    public void FindWithKind() {
        var t = Node(Leaf(0), Leaf(0));
        var output = F(t, Kind<Tree>(typeof(Tree.Node)));
        A(output, [[]]);
    }

    [Test]
    public void FindWithPredicate() { 
        var t = Leaf(77);
        var output = F(t, Predicate<Tree>(x => x is Tree.Leaf(Value: 77)));
        A(output, [[]]);
    }

    [Test]
    public void FindWithAnd() {
        var t = Leaf(77);
        var output = F(t, And<Tree>(Predicate<Tree>(x => x is Tree.Leaf(Value: 77)), Capture<Tree>("x")));
        A(output, [[("x", Leaf(77))]]);
    }

    [Test]
    public void FindWithOr() {
        var t = Leaf(15);
        var output = F(t, Or<Tree>(Wild<Tree>(), Wild<Tree>()));
        A(output, [[], []]);
    }

    [Test]
    public void FindWithMatchWith() {
        var t = Leaf(77);
        var output = F(t, MatchWith<Tree>( _ => Wild<Tree>()));
        A(output, [[]]);
    }

    // TODO
    // fail template with non existent var name
    // fail template with non matching value
    // anything with a switch to alt inside of it needs a failure test where it both does and does not switch to alt
    // {| [| a, ^ |] ; $a |} ~ [1, 1, 2, 2, 3, 3] => a = 1, a = 2, a = 3
    // Blah ( capture a, {| Other($a, ^, ^) ; ... |}) // And maybe also with first path item being a list path

    // Failures with no alternatives
    // the same failures with alternatives

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
