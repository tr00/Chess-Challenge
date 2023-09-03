using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using ChessChallenge.API;

using L = System.ArraySegment<object>;
using F = System.Func<object, object>;
using D = System.Collections.Generic.Dictionary<object, object>;

#nullable disable warnings

class Runtime {

    const byte nil_byte = 0xff;
    const byte pad_byte = 0x00;
    const byte lpa_byte = 0x01;
    const byte rpa_byte = 0x02;

    static D E;

    static object cons(object x, object y) {
        switch (y) {
            case L l: {
                var a = new object[l.Count + 1];
                a[0] = x;
                l.CopyTo(a, 1);
                return new L(a);
            }
            default: return new L(new[] { x, y });
        }
    }

    static object h(object x) => ((L)x)[0];

    static object t(object x) {
        var a = ((L)x);
        return a.Count switch {
            0 => throw new ArgumentException("somehow got empty list"),
            1 => a[0],
            _ => a.Slice(1),
        };
    }

    static object assoq(object x, D e) => 
        e.ContainsKey(x) ? e[x] : x;

    static object v(object x, D e) {
        L: // tail call optimization
    
        if (x is string) 
            return assoq(x, e);
        else if (x is L) {
            var f = v(h(x), e);
            var a = (L)t(x);

            if (f is string)
                switch ((string)f) {
                    case "v": x = a; goto L;
                    case "q": return a;
                    case "d": E[h(a)] = v(t(a), E); return h(a);
                    case "i": return v(v(a[0], e) != null ? a[1] : a[2], e);
                }
            
            a = (L)a.Select(y => v(y, e));

            if (f is F) return ((F)f)(a);

            e = new D(e);
            
            foreach ((var k, var v) in ((L)h(f)).Zip(a))
                e[k] = v;

            x = h(t(f));
                
            goto L;
        } else return x;
    }
    

    static void print(object x) {
        switch (x) {
            case null: Console.Write("nil"); break;
            case L a: {
                Console.Write("(");
                    
                switch (a.Count) {
                    case 1: print(a[0]); break;
                    case 2: {
                        print(a[0]);
                        Console.Write(" ");
                        print(a[1]);
                        break;
                    }
                    default: {
                        print(a[0]);
                        foreach (var obj in (L)t(a)) {
                            Console.Write(" ");
                            print(obj);
                        }
                        break;
                    }
                }

                Console.Write(")");
                break;
            }
            default: Console.Write(x); break;
        }
    }

    void Parse(decimal[] data) {
        // convert to byte array
        var code = data.SelectMany(decimal.GetBits)
            .Where(x => x != 0).SelectMany(BitConverter.ToByteArray);

        Stack<List<object>> stack = new();

        foreach (var token in code) {
            switch (token) {
                case pad_byte: continue;
                case nil_byte: stack.Peek().Add(null); break;
                case lpa_byte: stack.Push(new List<object>()); break;
                case rpa_byte: { 
                    var top = stack.Pop();
                    stack.Peek().Add(new L(top.ToArray()));
                    break;
                }
                default: stack.Peek().Add(token); break;
                // TODO: 
                // special treatment for int literals
                // maybe eol token
            }
        }
        
        // requires 1 extra leading lpa to work
        return stack.Peek()[0];
    }

    public static void Main() {
        // var b = Board.CreateBoardFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        // var x = cons("d", cons("abc", cons("q", 1)));


        E = new D {
            { "nil", null },
            { "b", b },
        };

        // v(x, E);

        print(t(cons("q", 1)));

        Console.WriteLine();
    }

}

#nullable enable warnings