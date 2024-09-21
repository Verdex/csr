
using csr.core.traverse;
using csr.core.code;
using System.Collections.Immutable;

namespace csr.core.parse;



public class CSharpPatternParser {
    private readonly Parser<Pattern<string, CSharpAst>> _parser;
    public CSharpPatternParser() {
        var lSquare = Letter('[');
        var rSquare = Letter(']');
        var lCurl = Letter('{');
        var rCurl = Letter('}');
        var lParen = Letter('(');
        var rParen = Letter(')');
        var captureVar = from s in Symbol() 
                         select new Pattern<string, CSharpAst>.Capture(s);
        var templateVar = from _ in Letter('$')
                          from s in Symbol(clearSpace: false)
                          select new Pattern<string, CSharpAst>.TemplateVar(s);


        _parser = captureVar.Or(templateVar);

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

    private static Result<Pattern<string, CSharpAst>> Succ(int index, Pattern<string, CSharpAst> pattern) 
        => new Result<Pattern<string, CSharpAst>>.Succ(index, pattern);
    private static Result<Pattern<string, CSharpAst>> Fail(int index, string message) 
        => new Result<Pattern<string, CSharpAst>>.Fail(index, message);

    private static Parser<Unit> Letter(char c, bool clearSpace = true) => new Parser<Unit>((input, index) => { 
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
        while (clearSpace && char.IsWhiteSpace(input[index])) {
            index += 1;
        }

        if (!char.IsAsciiLetter(input[index]) && input[index] != '_') {
            return new Result<string>.Fail(index, $"expected symbol character but found {input[index]}");
        }

        var sym = new string(input.Skip(index).TakeWhile(c => char.IsAsciiLetterOrDigit(c) || c == '_').ToArray());

        return new Result<string>.Succ(index + sym.Length, sym);
    });

    private sealed record Unit();
}

