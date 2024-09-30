
using csr.core.code;
using csr.core.parse;
using csr.core.traverse;

namespace csr.test.core.parse;

[TestFixture]
public class CSharpPatternParserTests {

    [TestCase("x", true)]
    [TestCase("_blah", true)]
    [TestCase("x1234", true)]
    [TestCase("x_1234", true)]
    [TestCase("_", false)]
    [TestCase("$_", false)]
    [TestCase("$_blarg", false)]
    [TestCase("$_blarg1234", false)]
    [TestCase("$x1234", false)]
    [TestCase(":x1234", false)]
    public void ShouldParseCapture(string input, bool isCapture) {
        var parser = Parser();
        
        var success = parser.TryParse(input, out var result);

        Assert.Multiple(() => {
            Assert.That(success, Is.True);

            var pattern = result as Pattern<string,CSharpAst>.Capture;
            Assert.That(pattern, isCapture ? Is.Not.Null : Is.Null);

            if(isCapture) {
                Assert.That(pattern!.Name, Is.EqualTo(input));
            }
        });
    }

    [TestCase("[a, b, c]", 3)]
    [TestCase("[[a], b, c]", 3)]
    public void ShouldParseContents(string input, int length) {
        var parser = Parser(); 

        var success = parser.TryParse(input, out var result);

        Assert.Multiple(() => {
            Assert.That(success, Is.True);

            var pattern = result as Pattern<string,CSharpAst>.Contents;
            Assert.That(pattern, Is.Not.Null);
            Assert.That(pattern!.Cs.Count, Is.EqualTo(length));
        });
    }

    [TestCase("x.and(y)")]
    [TestCase("x.and(y).and(z)")]
    [TestCase("x.and(y).or(z)")]
    [TestCase("x.or(y).or(z)")]
    [TestCase("x.or(y.or(w.and(h))).or(z)")]
    public void ShouldParseFollow(string input) {
        var parser = Parser();
        var success = parser.TryParse(input, out _);
        Assert.That(success, Is.True);
    }

    private static CSharpPatternParser Parser() => new();
}