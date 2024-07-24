
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
    private static void D(List<List<(string Name, Tree Item)>> input) {
        foreach( var i in input ) {
            foreach( var ilet in i ) {
                Console.Write(ilet);
            }
            Console.Write("\n");
        }
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

    [Test]
    public void FindWithSubContentPath() {
        var t = L(Leaf(1), Leaf(2), Leaf(3), Leaf(4));
        var output = F(t, SubContentPath<Tree>([Capture<Tree>("a"), Capture<Tree>("b")]));
        A(output, [ [("a", Leaf(1)), ("b", Leaf(2))]
                  , [("a", Leaf(2)), ("b", Leaf(3))]
                  , [("a", Leaf(3)), ("b", Leaf(4))]
                  ]);
    }

    [Test]
    public void FindWithPath() {
        var t = Node(Node(Leaf(1), Leaf(2)), Node(Leaf(3), Leaf(4)));
        var output = F(t, Path<Tree>([ Contents<Tree>([PathNext<Tree>(), PathNext<Tree>()])
                                     , Contents<Tree>([PathNext<Tree>(), PathNext<Tree>()])
                                     , Capture<Tree>("a")
                                     ]));
        A(output, [ [("a", Leaf(1))]
                  , [("a", Leaf(2))] 
                  , [("a", Leaf(3))] 
                  , [("a", Leaf(4))] 
                  ]);
    }

    [Test]
    public void FailTemplateWithUnknownName() {
        var t = Leaf(1);
        var output = F(t, TemplateVar<Tree>("x"));
        A(output, []);
    }

    [Test]
    public void FailTemplateWithNonMatchingValue() {
        var t = Node(Leaf(1), Leaf(2));
        var output = F(t, Contents<Tree>([Capture<Tree>("x"), TemplateVar<Tree>("x")]));
        A(output, []);
    }

    [Test]
    public void FindCaptureWithFirstOr() {
        var t = Leaf(1);
        var output = F(t, Or<Tree>(Capture<Tree>("a"), Exact<Tree>(typeof(int), [])));
        A(output, [[("a", Leaf(1))]]);
    }

    [Test]
    public void FindCaptureWithSecondOr() {
        var t = Leaf(1);
        var output = F(t, Or<Tree>(Exact<Tree>(typeof(int), []), Capture<Tree>("a")));
        A(output, [[("a", Leaf(1))]]);
    }

    [Test]
    public void FindCaptureWithBothOr() {
        var t = Leaf(1);
        var output = F(t, Or<Tree>(Capture<Tree>("a"), Capture<Tree>("a")));
        A(output, [[("a", Leaf(1))], [("a", Leaf(1))]]);
    }

    [Test]
    public void FailOr() {
        var t = Leaf(1);
        var output = F(t, Or<Tree>(Kind<Tree>(typeof(int)), Kind<Tree>(typeof(int))));
        A(output, []);
    }

    [Test]
    public void FailAndLeft() {
        var t = Leaf(1);
        var output = F(t, And<Tree>(Kind<Tree>(typeof(int)), Wild<Tree>()));
        A(output, []);
    }

    [Test]
    public void FailAndRight() {
        var t = Leaf(1);
        var output = F(t, And<Tree>(Wild<Tree>(), Kind<Tree>(typeof(int))));
        A(output, []);
    }

    [Test]
    public void FailAndBoth() {
        var t = Leaf(1);
        var output = F(t, And<Tree>(Kind<Tree>(typeof(int)), Kind<Tree>(typeof(int))));
        A(output, []);
    }

    [Test]
    public void FailContentsForLength() {
        var t = Node(Leaf(1), Leaf(1));
        var output = F(t, Contents<Tree>([Wild<Tree>()]));
        A(output, []);
    }

    [Test]
    public void FailExactForLength() {
        var t = Node(Leaf(1), Leaf(1));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [Wild<Tree>()]));
        A(output, []);
    }

    [Test]
    public void FailExactForType() {
        var t = Node(Leaf(1), Leaf(1));
        var output = F(t, Exact<Tree>(typeof(int), [Wild<Tree>(), Wild<Tree>()]));
        A(output, []);
    }

    [Test]
    public void FailPredicate() {
        var t = Leaf(1);
        var output = F(t, Predicate<Tree>(_ => false));
        A(output, []);
    }

    [Test]
    public void FailMatchWith() {
        var t = Leaf(1);
        var output = F(t, MatchWith<Tree>(_ => Predicate<Tree>(_ => false)));
        A(output, []);
    }

    [Test]
    public void FindSubContentPathInPath() {
        var t = L([Leaf(1), Leaf(1), Leaf(2), Leaf(2), Leaf(3), Leaf(3)]);
        var output = F(t, Path<Tree>([SubContentPath<Tree>([Capture<Tree>("a"), PathNext<Tree>()]), TemplateVar<Tree>("a")]));
        A(output, [[("a", Leaf(1))], [("a", Leaf(3))], [("a", Leaf(2))]]);
    }

    [Test]
    public void FindPathInSubContentPath() {
        var t = L([Node(Leaf(1), Leaf(2)), Leaf(3), Node(Leaf(4), Leaf(5)), Node(Leaf(5), Leaf(6)), Node(Leaf(6), Leaf(7))]);
        var output = F(t, SubContentPath<Tree>([ Path<Tree>([Exact<Tree>(typeof(Tree.Node), [Wild<Tree>(), Capture<Tree>("a")])])
                                               , Path<Tree>([Exact<Tree>(typeof(Tree.Node), [TemplateVar<Tree>("a"), Capture<Tree>("b")])])
                                               ]));
        A(output, [[("a", Leaf(5)), ("b", Leaf(6))], [("a", Leaf(6)), ("b", Leaf(7))]]);
    }

    [Test]
    public void FindSubContentPathWithThreeCaptures() {
        var t = L([Leaf(1), Leaf(2), Leaf(3), Leaf(4), Leaf(5)]);
        var output = F(t, SubContentPath<Tree>([ Capture<Tree>("a"), Capture<Tree>("b"), Capture<Tree>("c") ]));
        A(output, [ [ ("a", Leaf(1)), ("b", Leaf(2)), ("c", Leaf(3)) ]
                  , [ ("a", Leaf(2)), ("b", Leaf(3)), ("c", Leaf(4)) ]
                  , [ ("a", Leaf(3)), ("b", Leaf(4)), ("c", Leaf(5)) ]
                  ]);
    }

    [Test]
    public void UseCaptureFromExactPatternInPath() {
        var t = Node(Leaf(1), L(Leaf(1), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), 
            Path<Tree>([Exact<Tree>(typeof(Tree.L), [TemplateVar<Tree>("a"), PathNext<Tree>(), PathNext<Tree>()])
                       , Capture<Tree>("b")
                       ])]));
        A(output, [ [("a", Leaf(1)), ("b", Leaf(2))]
                  , [("a", Leaf(1)), ("b", Leaf(3))] 
                  ]);
    }

    [Test]
    public void UsePathCapturesFromExactPatternInPath() { 
        var t = Node(Node(Node(Leaf(1), Leaf(4)), Node(Leaf(1), Leaf(5))), L(Leaf(1), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [ Path<Tree>( [ Exact<Tree>(typeof(Tree.Node), [PathNext<Tree>(), PathNext<Tree>()])
                                                                       , Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), Capture<Tree>("c")])
                                                                       ]), 
            Path<Tree>([Exact<Tree>(typeof(Tree.L), [TemplateVar<Tree>("a"), PathNext<Tree>(), PathNext<Tree>()])
                       , Capture<Tree>("b")
                       ])]));
        A(output, [ [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(2))]
                  , [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(3))] 
                  , [("a", Leaf(1)), ("c", Leaf(5)), ("b", Leaf(2))] 
                  , [("a", Leaf(1)), ("c", Leaf(5)), ("b", Leaf(3))] 
                  ]);
    }

    [Test]
    public void UsePathCapturesFromExactPatternInPathWithFailingInitialCaptures() { 
        var t = Node(Node(Node(Leaf(1), Leaf(4)), Node(Leaf(2), Leaf(5))), L(Leaf(1), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [ Path<Tree>( [ Exact<Tree>(typeof(Tree.Node), [PathNext<Tree>(), PathNext<Tree>()])
                                                                       , Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), Capture<Tree>("c")])
                                                                       ]), 
            Path<Tree>([Exact<Tree>(typeof(Tree.L), [TemplateVar<Tree>("a"), PathNext<Tree>(), PathNext<Tree>()])
                       , Capture<Tree>("b")
                       ])]));
        A(output, [ [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(2))]
                  , [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(3))] 
                  ]);
    }

    [Test]
    public void UsePathCapturesFromExactPatternInPathWithFailingSecondaryCaptures() { 
        var t = Node(Node(Node(Leaf(1), Leaf(4)), Node(Leaf(1), Leaf(5))), L(Leaf(0), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [ Path<Tree>( [ Exact<Tree>(typeof(Tree.Node), [PathNext<Tree>(), PathNext<Tree>()])
                                                                       , Exact<Tree>(typeof(Tree.Node), [Capture<Tree>("a"), Capture<Tree>("c")])
                                                                       ]), 
            Path<Tree>([Exact<Tree>(typeof(Tree.L), [TemplateVar<Tree>("a"), PathNext<Tree>(), PathNext<Tree>()])
                       , Capture<Tree>("b")
                       ])]));
        A(output, []); 
    }


    [Test]
    public void UseSubContentPathCapturesFromExactPatternInPath() { 
        var t = Node(L(Leaf(1), Leaf(4), Leaf(1), Leaf(5)), L(Leaf(1), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [ SubContentPath<Tree>( [ Capture<Tree>("a"), Capture<Tree>("c") ] ),
            Path<Tree>([Exact<Tree>(typeof(Tree.L), [TemplateVar<Tree>("a"), PathNext<Tree>(), PathNext<Tree>()])
                       , Capture<Tree>("b")
                       ])]));
        A(output, [ [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(2))]
                  , [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(3))] 
                  , [("a", Leaf(1)), ("c", Leaf(5)), ("b", Leaf(2))] 
                  , [("a", Leaf(1)), ("c", Leaf(5)), ("b", Leaf(3))] 
                  ]);
    }

    [Test]
    public void UseSubContentPathCapturesFromExactPatternInSubContentPath() {
        var t = Node(L(Leaf(1), Leaf(4), Leaf(2), Leaf(5)), L(Leaf(1), Leaf(2), Leaf(3)));
        var output = F(t, Exact<Tree>(typeof(Tree.Node), [ SubContentPath<Tree>( [ Capture<Tree>("a"), Capture<Tree>("c") ] ),
            SubContentPath<Tree>([TemplateVar<Tree>("a"), Capture<Tree>("b")])]));
        A(output, [ [("a", Leaf(1)), ("c", Leaf(4)), ("b", Leaf(2))]
                  , [("a", Leaf(2)), ("c", Leaf(5)), ("b", Leaf(3))] 
                  ]);
    }

    [Test]
    public void SubContentPathShouldFailForShortData() {
        var t = Node(Leaf(1), Leaf(2));
        var output = F(t, SubContentPath<Tree>([Wild<Tree>(), Wild<Tree>(), Wild<Tree>()]));
        A(output, []);
    }

    [Test]
    public void SubContentPathShouldFailForShortDataInAlternative() {
        var t = Node(Leaf(1), Leaf(2));
        var output = F(t, Or<Tree>(SubContentPath<Tree>([Wild<Tree>(), Wild<Tree>(), Wild<Tree>()]), Capture<Tree>("a")));
        A(output, [[("a", Node(Leaf(1), Leaf(2)))]]);
    }

    [Test]
    public void ContentShouldFailForUnequalDataInAlternative() {
        var t = Node(Leaf(1), Leaf(2));
        var output = F(t, Or<Tree>(Contents<Tree>([Capture<Tree>("a")]), Contents<Tree>([Capture<Tree>("b"), Capture<Tree>("c")])));
        A(output, [[("b", Leaf(1)), ("c", Leaf(2))]]);
    }

    // zero length path

    // TODO
    // anything with a switch to alt inside of it needs a failure test where it both does and does not switch to alt

    // Failures with no alternatives
    // the same failures with alternatives
    

    // Match with that looks at captures and works differently for different alternatives b/c captures change

    private static Tree Leaf(byte input) => new Tree.Leaf(input);
    private static Tree Node(Tree left, Tree right) => new Tree.Node(left, right);
    private static Tree L(params Tree[] xs) => new Tree.L(xs.ToList());

    private abstract record Tree : IMatchable<Tree> {
        private Tree() { }

        public record Leaf(byte Value) : Tree;
        public record Node(Tree Left, Tree Right) : Tree;
        public record L(List<Tree> Items) : Tree;

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
                case L l:
                    id = typeof(L);
                    contents = new List<Tree>(l.Items);
                    break;
                default:
                    throw new NotImplementedException($"Unknown {nameof(Tree)} case {this.GetType()}");
            }
        }
    }
}
