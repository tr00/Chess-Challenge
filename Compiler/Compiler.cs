using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Compiler {

    /**
     * quick bytecode explanation:
     *
     *      nil := 0xff
     *      pad := 0x00 // these tokens are ignored when parsed
     *      '(' := 0x01
     *      ')' := 0x02
     *     eval := 0x03
     *    quote := 0x04
     *   define := 0x05 // binds in global scope (env)
     *       if := 0x06
     *     cons := 0x07
     *      car := 0x08
     *      cdr := 0x09
     *    board := 0x0a
     *    timer := 0x0b
     *    moves := 0x0c
     *
     * the rest are symbol names
     */

    const byte nil_byte = 0xff;
    const byte pad_byte = 0x00;
    const byte lpa_byte = 0x01;
    const byte rpa_byte = 0x02;

    interface Expr {};

    record Nil : Expr;
    record Number(ulong value) : Expr;
    record Symbol(byte id) : Expr;
    record Cons(List<Expr> list) : Expr;
    record Dict(Dictionary<Expr, Expr> dict) : Expr;

    interface Token {};

    record struct LParen : Token;
    record struct RParen : Token;

    record struct Identifier(string s) : Token;
    record struct IntLiteral(string s) : Token;

    Regex space, alpha, digit;

    Dictionary<string, byte> symbols;

    Nil nil;


    public Compiler() {
        space = new Regex(@"\G\s+");
        alpha = new Regex(@"\G[a-zA-Z\-_]+");
        digit = new Regex(@"\G\d+");

        symbols = new Dictionary<string, byte> {
            { "eval",   0x03 },
            { "quote",  0x04 },
            { "define", 0x05 },
            { "if",     0x06 },
            { "cons",   0x07 },
            { "car",    0x08 },
            { "cdr",    0x09 },
            { "board",  0x0a },
            { "timer",  0x0b },
            { "moves",  0x0c },
        };

        nil = new Nil();
    }


    Token? NextToken(string source, ref int offset) {
        // eat spaces
        var match = space.Match(source, offset);

        offset += match.Length;

        if (source[offset] == '(') {
            offset++;
            return new LParen();
        }

        if (source[offset] == ')') {
            offset++;
            return new RParen();
        }

        match = alpha.Match(source, offset);

        if (match.Success) {
            int start = offset;
            offset += match.Length;
            return new Identifier(source.Substring(start, match.Length));
        }

        match = digit.Match(source, offset);

        if (match.Success) {
            int start = offset;
            offset += match.Length;
            return new IntLiteral(source.Substring(start, match.Length));
        }
        
        return null;
    }

    Expr Parse(string source) {
        int offset = 0;

        var stack = new Stack<Cons>();

        while (offset < source.Length) {
            var token = NextToken(source, ref offset);

            switch (token) {
                case null: {
                    if (stack.Count == 1) {
                        return stack.Peek();
                    } else {
                        throw new ArgumentException("error while parsing");
                    }
                }
                
                case LParen: 
                    stack.Push(new Cons(new List<Expr>()));
                    break;

                case RParen: {
                    var top = stack.Pop();
                    if (stack.Count == 0)
                        return top;
                    stack.Peek().list.Add(top);
                    break;
                }

                case Identifier x: {
                    if (x.s == "nil") {
                        stack.Peek().list.Add(nil);
                    }

                    if (!symbols.ContainsKey(x.s))
                        symbols[x.s] = (byte)(symbols.Count + 3);

                    byte id = symbols[x.s];

                    stack.Peek().list.Add(new Symbol(id));
                    break;
                }

                case IntLiteral x:
                    stack.Peek().list.Add(new Number(UInt64.Parse(x.s)));
                    break;
            }
        }

        throw new ArgumentException("premature end of input");
    }

    void EmitPrefix(List<byte> buffer) {
        // extra leading lpa saves tokens
        // in the bytecode parser
        buffer.Add(lpa_byte);
    }

    void EmitSuffix(List<byte> buffer) {
        Console.WriteLine("padding: {0} bytes", 12 - (buffer.Count % 12));

        while (buffer.Count % 12 != 0)
            buffer.Add(pad_byte);
    }

    void EmitExpression(Expr expr, List<byte> buffer) {
        switch (expr) {
            case Nil: buffer.Add(nil_byte); break;
            case Cons as_cons: {
                buffer.Add(lpa_byte);
                
                foreach (var child in as_cons.list)
                    EmitExpression(child, buffer);

                buffer.Add(rpa_byte);
                break;
            }
            case Symbol as_symbol:
                buffer.Add(as_symbol.id);
                break;
        }
    }

    IEnumerable<decimal> ConvertBytesToDecimals(List<byte> src) {
        var dst = src
            .Chunk(4)
            .Select(x => BitConverter.ToInt32(x))
            .Chunk(3)
            .Select(x => new decimal(x[0], x[1], x[2], false, 0));

        return dst;
    }

    void Compile(Expr expr) {
        List<byte> buffer = new List<byte>();

        EmitPrefix(buffer);

        EmitExpression(expr, buffer);

        EmitSuffix(buffer);

        var decimals = ConvertBytesToDecimals(buffer);

        Console.WriteLine("\nbytecode:");
        foreach (var dec in decimals) {
            Console.WriteLine("{0}m,", dec);
        }

        Console.WriteLine("\nsymbols:");
        foreach ((var key, var val) in symbols) {
            Console.WriteLine("[\"{0}\"] = 0x{1:x2}", key, val);
        }

    }

    static void Main() {
        var compiler = new Compiler();

        var ast = compiler.Parse("(quote (car (moves board)))");
        compiler.Compile(ast);
    }


}