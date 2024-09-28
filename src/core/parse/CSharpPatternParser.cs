
using csr.core.traverse;
using csr.core.code;
using System.Collections.Immutable;

namespace csr.core.parse;



public class CSharpPatternParser {
    private readonly Parser<Pattern<string, CSharpAst>> _parser;
    public CSharpPatternParser() {
        var topLevel = TopLevel();

        var lSquare = Letter('[');
        var rSquare = Letter(']');
        var lCurl = Letter('{');
        var rCurl = Letter('}');
        var lParen = Letter('(');
        var rParen = Letter(')');
        var comma = Letter(',');
        var wild = from s in Symbol() 
                   where s == "_"
                   select Wild();
        var capture = from s in Symbol() 
                      select Capture(s);
        var templateVar = from _ in Letter('$')
                          from s in Symbol(clearSpace: false)
                          select TemplateVar(s);

        var patternComma = from p in topLevel 
                           from _ in comma
                           select p;

        var patternList = from pc in patternComma.ZeroOrMore()
                          from p in topLevel 
                          select pc.Append(p);

        var contents = from _1 in lSquare
                       from ps in patternList
                       from _2 in rSquare
                       select Contents(ps.ToImmutableList());

        _parser = contents.Or(wild, capture, templateVar);

    }

    public bool TryParse(string input, out Pattern<string, CSharpAst> pattern) {
        switch (_parser.Parse(input)) {
            case Result<Pattern<string, CSharpAst>>.Succ s:
                pattern = s.Item;
                return true;
            case Result<Pattern<string, CSharpAst>>.Fail:
                pattern = new Pattern<string, CSharpAst>.Wild();
                return false;
            default:
                throw new Exception("Encountered unknown Result in CSharpPatternParser::TryParse");
        }
    }

    private static Parser<Unit> Letter(char c, bool clearSpace = true) => new Parser<Unit>((input, index) => { 
        if (index >= input.Length) {
            return new Result<Unit>.Fail(index, $"expected End Of Input encountered");
        }

        while (clearSpace && char.IsWhiteSpace(input[index])) {
            index += 1;
        }

        if(input[index] == c) {
            return new Result<Unit>.Succ(index + 1, new Unit());
        }
        else {
            return new Result<Unit>.Fail(index, $"expected {c} but found {input[index]}");
        }
    });

    private static Parser<string> Symbol(bool clearSpace = true) => new Parser<string>((input, index) => { 
        if (index >= input.Length) {
            return new Result<string>.Fail(index, $"expected End Of Input encountered");
        }

        while (clearSpace && char.IsWhiteSpace(input[index])) {
            index += 1;
        }

        if (!char.IsAsciiLetter(input[index]) && input[index] != '_') {
            return new Result<string>.Fail(index, $"expected symbol character but found {input[index]}");
        }

        var sym = new string(input.Skip(index).TakeWhile(c => char.IsAsciiLetterOrDigit(c) || c == '_').ToArray());

        return new Result<string>.Succ(index + sym.Length, sym);
    });

    private Parser<Pattern<string, CSharpAst>> TopLevel() => new ((input, index) => _parser.P(input, index));

    private sealed record Unit();

    private static Pattern<string, CSharpAst> Wild() => new Pattern<string, CSharpAst>.Wild();
    private static Pattern<string, CSharpAst> Exact(string t, ImmutableList<Pattern<string, CSharpAst>> contents) => new Pattern<string, CSharpAst>.Exact(t, contents);
    private static Pattern<string, CSharpAst> Contents(ImmutableList<Pattern<string, CSharpAst>> contents) => new Pattern<string, CSharpAst>.Contents(contents);
    private static Pattern<string, CSharpAst> Kind(string t) => new Pattern<string, CSharpAst>.Kind(t);
    private static Pattern<string, CSharpAst> And(Pattern<string, CSharpAst> a, Pattern<string, CSharpAst> b) => new Pattern<string, CSharpAst>.And(a, b);
    private static Pattern<string, CSharpAst> Or(Pattern<string, CSharpAst> a, Pattern<string, CSharpAst> b) => new Pattern<string, CSharpAst>.Or(a, b);
    private static Pattern<string, CSharpAst> Capture(string s) => new Pattern<string, CSharpAst>.Capture(s);
    private static Pattern<string, CSharpAst> TemplateVar(string s) => new Pattern<string, CSharpAst>.TemplateVar(s);
    private static Pattern<string, CSharpAst> SubContentPath(ImmutableList<Pattern<string, CSharpAst>> contents) => new Pattern<string, CSharpAst>.SubContentPath(contents);
    private static Pattern<string, CSharpAst> PathNext() => new Pattern<string, CSharpAst>.PathNext();
    private static Pattern<string, CSharpAst> Path(ImmutableList<Pattern<string, CSharpAst>> contents) => new Pattern<string, CSharpAst>.Path(contents);
    private static Pattern<string, CSharpAst> Predicate(Func<CSharpAst, bool> predicate) => new Pattern<string, CSharpAst>.Predicate(predicate);
    private static Pattern<string, CSharpAst> MatchWith(Func<IReadOnlyDictionary<string, CSharpAst>, Pattern<string, CSharpAst>> func) => new Pattern<string, CSharpAst>.MatchWith(func);

}

