using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using ChessChallenge.API;

using L = System.Collections.Generic.IEnumerable<object>;
using F = System.Func<object, object>;
using D = System.Collections.Generic.Dictionary<object, object>;

#nullable disable warnings

class Runtime {

    object env;

    IEnumerable<byte> tokenize(decimal[] decimals) {
        var result = decimals.SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes);

        return result;
    }

    object parse(IEnumerable<byte> tokens) {
        Stack<List<object>> stack = new();

        foreach (var token in tokens)
        {
            switch (token) {
                case 0x00: continue;
                case 0x01: // lpa
                    stack.Push(new()); 
                    break;
                case 0x02: // rpa
                    var top = stack.Pop();
                    stack.Peek().Add(top);
                    break;
                default:
                    stack.Peek().Add((int)token);
                    break;
            }
        }

        ast = stack.Peek()[0];

        return ast;
    }

    object cons(object car, object cdr) {
        return cdr switch {
            L l => l.Prepend(car);
            _ => new object[] { car, cdr };
        };
    }

    object car(object x) => ((L)x).First();
    object cdr(object x) => ((L)x).Skip(1);

    object nilq(object x) => x switch {
        L l => !l.Any(),
        _ => false,
    };


    object eval(object x, D e, int d = 0) {
        TCO:
        debug(d, "> eval ", x);

        if (nilq(x))
        {
            debug(d, "> yield 'nilq' ", x);
            return x;
        }

        if (x is not L)
        {
            if (e.ContainsKey(x))
            {
                var tmp = e[x];
                debug(d, "> yield 'env' ", tmp);
                return tmp;
            }
            else
            {
                debug(d, "> yield 'id' ", x);
                return x;
            }
        }

        debug(d, "> func ", car(x));
        var func = eval(car(x), e, d + 1);
        var args = cdr(x);

        if (func is int)
        {
            switch ((int)func)
            {
                case 0x05: // eval
                    debug(d, "> evalf ", car(args));
                    x = eval(car(args), e, d + 1);
                    d++;
                    goto TCO;
                
                case 0x06: // quote
                    debug(d, "> yield 'quote' ", car(args));
                    return car(args);

                case 0x07: // define
                    
                    var res = eval(car(cdr(args)), env, d + 1);
                
                case 0x08: // if
                    debug(d, "> if ", car(args));
                    var cond_expr = eval(car(args), e, d + 1);
                    var then_expr = car(cdr(args));
                    var else_expr = car(cdr(cdr(args)));

                    if ((bool)cond_expr)
                    {
                        debug(d, "> then ", then_expr);
                        x = then_expr;
                    }
                    else
                    {
                        debug(d, "> else ", else_expr);
                        x = else_expr;
                    }

                    d++;
                    goto TCO;

                default:
                    throw new ArgumentException($"got int ({func}) but its not a builtin macro!");
            }
        }

    }

    void debug(int d, string p, object x = null) {
        Console.Write(String.Concat(Enumerable.Repeat("  ", )), p);
        print(x);
    }

    void print(object x) {
        switch (x) {
            case L a: {
                Console.Write("(");

                if (!nilq(a))
                    print(car(a));

                a = (L)cdr(a);

                while (!nilq(a)) {
                    Console.Write(" ");
                    print(car(a));
                    a = (L)cdr(a);
                }

                Console.Write(")");
                break;
            }

            case int:
                Console.Write("0x{0:x2}", x);
                break;

            default:
                Console.Write(x);
                break;
        }
    }

}

#nullable enable warnings