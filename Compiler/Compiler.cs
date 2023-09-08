using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Compiler {

    const long KEY = 0x306fc9df731d49e;

    const byte pad_byte = 0x00;
    const byte lpa_byte = 0x01;
    const byte rpa_byte = 0x02;
    const byte i64_byte = 0x03;

    interface Expr {};

    record Nil : Expr;
    record Number(long value) : Expr;
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
        digit = new Regex(@"\G\-?\d+");

        symbols = new Dictionary<string, byte> {
            // entry point
            { "_start", 0xff },

            // special forms
            { "eval",   0x05 },
            { "quote",  0x06 },
            { "define", 0x07 },
            { "if",     0x08 },
            { "let",    0x09 },

            // core library
            { "cons",   0x0a },
            { "car",    0x0b },
            { "cdr",    0x0c },
            { "nilq",   0x0d },
            { "print",  0x0e },
            { "<r0f>",  0x0f },

            { "zero",   0x10 },
            { "one",    0x11 },
            { "<r12>",  0x12 },
            { "<r13>",  0x13 },

            // arithmetic
            { "add",    0x14 },
            { "sub",    0x15 },
            { "mul",    0x16 },
            { "div",    0x17 },

            { "eq",     0x18 },
            { "ge",     0x19 },
            { "lt",     0x1a },
            { "<r1b>",  0x1b },
            { "<r1c>",  0x1c },
            { "<r1d>",  0x1d },
            { "<r1e>",  0x1e },
            { "<r1f>",  0x1f },

            // chess api
            { "get-moves",      0x20 },
            { "get-pieces",     0x21 },
            { "side-to-move",   0x22 },
            { "make-move",      0x23 },
            { "undo-move",      0x24 },
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

        match = digit.Match(source, offset);

        if (match.Success) {
            int start = offset;
            offset += match.Length;
            return new IntLiteral(source.Substring(start, match.Length));
        }

        match = alpha.Match(source, offset);

        if (match.Success) {
            int start = offset;
            offset += match.Length;
            return new Identifier(source.Substring(start, match.Length));
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
                        symbols[x.s] = (byte)(symbols.Count + 4);

                    byte id = symbols[x.s];

                    stack.Peek().list.Add(new Symbol(id));
                    break;
                }

                case IntLiteral x:
                    long value = Int64.Parse(x.s) ^ KEY;
                    if ((value & (0xffffffff << 0)) == 0 ||
                        (value & (0xffffffff << 1)) == 0 ||
                        (value & (0xffffffff << 2)) == 0 ||
                        (value & (0xffffffff << 3)) == 0 ||
                        (value & (0xffffffff << 4)) == 0)
                        Console.WriteLine("WARNING: Unsafe key was used! Data could corrupt!");

                    stack.Peek().list.Add(new Number(Int64.Parse(x.s) ^ KEY));
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
        Console.WriteLine("xor-key: 0x{0:x}", KEY);
        Console.WriteLine("padding: {0} bytes", 13 - (buffer.Count % 12));

        // padding with leading lpa's reduces tokens
        // compared to introducing an extra padding byte
        while (buffer.Count % 12 != 0)
            buffer.Insert(0, lpa_byte);

    }

    void EmitExpression(Expr expr, List<byte> buffer) {
        switch (expr) {
            // case Nil: buffer.Add(nil_byte); break;
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
            case Number as_number:
                // we have to make sure there is 9 bytes space
                // before the next decimal flag section
                // so that our data doesnt get corrupted
                // while (buffer.Count % 12 > 3)
                    // buffer.Add(pad_byte);

                buffer.Add(i64_byte);
                buffer.AddRange(BitConverter.GetBytes(as_number.value));
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

        var prog = File.ReadAllText("bots/bot-v4.lisp");

        var ast = compiler.Parse(prog);
        compiler.Compile(ast);
    }

}