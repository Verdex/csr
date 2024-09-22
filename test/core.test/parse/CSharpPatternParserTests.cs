
using csr.core.code;
using csr.core.parse;
using csr.core.traverse;

namespace csr.test.core.parse;

[TestFixture]
public class CSharpPatternParserTests {

    [TestCase("x", true)]
    [TestCase("_blah", true)]
    [TestCase("_", false)]
    [TestCase("x1234", true)]
    [TestCase("x_1234", true)]
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

    private static CSharpPatternParser Parser() => new();
}