
// #define VEGETABLES
#define DEBUGINFO

using ChessChallenge.API;

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using L = System.Collections.Generic.List<object>;
using F = System.Func<object, object>;
using D = System.Collections.Generic.Dictionary<object, object>;

/**
 * This bot my not be the most challenging chess opponent
 * but it can surely make your brain tickle when tryying
 * to make sense of its internals :]
 * 
 * Here is a broad overview of what is happening:
 * 
 * 1.   The actual chess bot (search + eval) is implemented in
 *      tiny lisp. When asked to think an interpreter runs the bot.
 *
 * 2.   This lisp code is converted to a bytecode representation
 *      to simplify stuff and make the code just raw data.
 * 
 * 3.   The bytecode is packed together in C#'s decimal literals
 *      and unpacked & parsed once in the constructor.
 *
 */
public class MyBot : IChessBot
{
    D env;
    object nil;

    // this type system is pure pain
    object binop_factory(int id) => 
        (object x) => { 
            var l = (L)x;
            return binop((long)(((L)x)[0]), (long)(((L)x)[1]), id);
        };

    object binop(long a, long b, int id) => id switch {
        0x14 => a + b,
        0x15 => a - b,
        0x16 => a * b,
        0x17 => a / b,
    };
    

    public MyBot() {
        nil = new L();

        /* core library */
        env = new D {
            { 0x0a, (object x) => cons(car(x), car(cdr(x))) },
            { 0x0b, (object x) => car(car(x)) },
            { 0x0c, (object x) => cdr(car(x)) },
            { 0x0d, (object x) => (object)nilq(car(x)) },
            // TODO: eq


            { 0x10, 0L },
            { 0x11, 1L },

            // { 0x14, (object x) => (object)((long)(((L)x)[0]) + (long)(((L)x)[1])) },
            // { 0x15, (object x) => (object)((long)(((L)x)[0]) - (long)(((L)x)[1])) },
            // { 0x16, (object x) => (object)((long)(((L)x)[0]) * (long)(((L)x)[1])) },
            // { 0x17, (object x) => (object)((long)(((L)x)[0]) / (long)(((L)x)[1])) },
            { 0x14, binop_factory(0x14) },
            { 0x15, binop_factory(0x15) },
            { 0x16, binop_factory(0x16) },
            // { 0x17, binop_factory(0x17) },

            { 0x20, (object x) => (object)((Board)car(x)).GetLegalMoves().Cast<object>().ToList() },
            { 0x21, (object x) => {
                var board = (Board)car(x);
                var pieces = board.GetAllPieceLists();
                return pieces.Select((PieceList x) => (object)x.Cast<object>().ToList()).ToList();
            } },
                // (object)((Board)car(x)).GetAllPieceLists().Select(Enumerable.ToList).ToList() },
            { 0x22, (object x) => (object)((Board)car(x)).IsWhiteToMove },
        };


#if VEGETABLES
        vegetables(); // #DEBUG
#endif

        Stack<L> stack = new();

        var code = new [] {

310722290811151159421632769m,
1864168520753147871783813378m,
621393276898830033439293698m,
2786583495982853380571791618m,
4024551851031407363577020710m,
12115859326395243453303226921m,
13009255504277675912408596776m,
12691308163886773106995438337m,
2477099205521758442924081666m,
3715232125111872688772156673m,
316744099411056985603441193m,
66188702846844079088604609027m,
48887986559787195198206768945m,
936901534686478356736837372m,
47890401641157051900252068355m,
30340175969899163026399819569m,
78291260979577858996406384899m,
13174892415782477060022872067m,
30340175930700106687657937102m,
78291260979577924005031377155m,
312125661317407102197629442m,
13309168260088270747528397064m,
13928043852796400861163948801m,
621397353065730992960834561m,
13928138278447418639484193025m,
678367964287713223975706881m,
78929562596224175614792630545m,
678358519770920293354251009m,
621397353053053628184592898m,

        }.SelectMany(decimal.GetBits).Where(x => x != 0).SelectMany(BitConverter.GetBytes).ToArray();

        for (int i = 0; i < code.Length; ++i)
            switch (code[i]) {
                // case 0x00: continue;
                case 0x01: // lpa
                    stack.Push(new()); 
                    break;
                case 0x02: // rpa
                    var top = stack.Pop();
                    stack.Peek().Add(top);
                    break;
                case 0x03: // i64
                    stack.Peek().Add(BitConverter.ToInt64(code, i + 1) ^ 0x306fc9df731d49e);
                    i += 8;
                    break;
                default:
                    stack.Peek().Add((int)code[i]);
                    break;
            }

        eval(stack.Peek()[0], env);
    }

    // L as_list(object x) => (L)x;

    object cons(object car, object cdr) => ((L)cdr).Prepend(car).ToList();

    object car(object x) => ((L)x).First();

    object cdr(object x) => ((L)x).Skip(1).ToList();

    bool nilq(object x) => x switch {
        L l => !l.Any(),
        _ => false,
    };

// ------------------------------------------------------------
// ------------------------------------------------------------
// ------------------------------------------------------------
// ------------------------------------------------------------

    object eval(object x, D e) {
        TCO:

#if DEBUGINFO
        Console.Write("eval: ");    // #DEBUG
        print(x);                   // #DEBUG
        Console.WriteLine();        // #DEBUG
#endif

        if (x is not L)
            return e.ContainsKey(x) ? e[x] : x;

        if (nilq(x))
            return x;


        var func = eval(car(x), e);
        var args = (L)cdr(x);

        var args0 = args[0];

        if (func is int)
            switch (func) {
                case 0x05: // eval
                    x = eval(args0, e);
                    goto TCO;

                case 0x06: // quote
                    return args0;

                // case 0x07: // define
                //     var tmp = eval(args[1], env);
                //     env[args0] = tmp;
                //     return tmp;

                case 0x08: // if
                    x = (bool)eval(args0, e) ? args[1] : args[2];
                    goto TCO;

                case 0x09: // let
                    e[args0] = eval(args[1], e);
                    x = args[2];
                    goto TCO;
#if DEBUGINFO
                default:  // #DEBUG
                    foreach(var pair in env) // #DEBUG
                        Console.WriteLine("key: {0}", pair.Key); // #DEBUG
                    throw new ArgumentException($"unrecognized symbol: {func}"); // #DEBUG
#endif
            }

        // this is a lazy iterator
        // for macros its never realized
        var iter = args.Select(arg => eval(arg, e));

        if (func is F) 
            return ((F)func)(iter.ToList());

        var list = (L)func;

        if (!nilq(car(func)))
            args = iter.ToList();


#if DEBUGINFO
        Console.Write("lambda: ");      // #DEBUG
        print(func);                    // #DEBUG
        Console.WriteLine();            // #DEBUG
#endif

        // e = ((L)list[^2]).Zip(args).ToDictionary(pair => pair.First, pair => pair.Second);
        x = list[^1];

        e = new(e);
        foreach ((var k, var v) in ((L)list[^2]).Zip(args))
            e[k] = v;
        // x = car(cdr(func));

        goto TCO;
    }

    public Move Think(Board board, Timer timer) {
        Console.WriteLine("thinking..."); // #DEBUG
        // Console.WriteLine("material-heuristic: {0}", eval(new L{0x30, board}, env)); // #DEBUG
        return (Move)eval(new L{0xff, board, timer}, env);
    }

    // debugging:

    void print(object x) {                                  // #DEBUG 
        switch (x) {                                        // #DEBUG
            case null: Console.WriteLine("nil"); break;     // #DEBUG
            case L a: {                                     // #DEBUG
                Console.Write("(");                         // #DEBUG
                if (!nilq(a))                               // #DEBUG
                    print(car(a));                          // #DEBUG
                a = (L)cdr(a);                              // #DEBUG
                while (!nilq(a)) {                          // #DEBUG
                    Console.Write(" ");                     // #DEBUG
                    print(car(a));                          // #DEBUG
                    a = (L)cdr(a);                          // #DEBUG
                }                                           // #DEBUG
                Console.Write(")"); break;                  // #DEBUG
                }                                           // #DEBUG
            case int:                                       // #DEBUG
                Console.Write("0x{0:x2}", x); break;        // #DEBUG
            default: Console.Write(x); break;               // #DEBUG
        }                                                   // #DEBUG
    }                                                       // #DEBUG

#if VEGETABLES

/*

    void vegetables()
    {
        test_builtins();

        Console.WriteLine("passed all tests!");
    }

    void test_builtins() {
        object res, tmp;

        // identity
        res = eval(0x42, new D());
        assume(res, 0x42);

        // nil handling
        res = eval(nil, new D());
        assume(nilq(res), true);

        // lookup
        res = eval(0xab, new D {{0xab, 0xcd}});
        assume(res, 0xcd);

        // quote 1
        res = eval(cons(0x06, cons(0x55, nil)), new D());
        assume(res, 0x55);

        // quote 2
        tmp = cons(0xab, nil);
        res = eval(cons(0x06, cons(tmp, nil)), new D());
        assume(res, tmp);

        // quote 3
        tmp = cons(0xfe, cons(0xdc, nil));
        res = eval(cons(0x06, cons(tmp, nil)), new D());
        assume(res, tmp);

        // if 1
        tmp = cons(0x08, cons(true, cons(0xab, cons(0xcd, nil))));
        res = eval(tmp, new D());
        assume(res, 0xab);
        
        tmp = cons(0x08, cons(false, cons(0xab, cons(0xcd, nil))));
        res = eval(tmp, new D());
        assume(res, 0xcd);

        // if 2
        tmp = cons(0x08, cons(true, cons(cons(0x06, cons(0xab, nil)), cons(0xcd, nil))));
        res = eval(tmp, new D());
        assume(res, 0xab);

        tmp = cons(0x08, cons(false, cons(0xab, cons(cons(0x06, cons(0xcd, nil)), nil))));
        res = eval(tmp, new D());
        assume(res, 0xcd);

        // if 3
        tmp = cons(0x08, cons(cons(0x06, cons(true, nil)), cons(0xab, cons(0xcd, nil))));
        res = eval(tmp, new D());
        assume(res, 0xab);

        tmp = cons(0x08, cons(cons(0x06, cons(false, nil)), cons(0xab, cons(0xcd, nil))));
        res = eval(tmp, new D());
        assume(res, 0xcd);

        // define 1  // (d 0xfe 0x33) ; (v 0xfe)
        eval(cons(0x07, cons(0xfe, cons(0x33, nil))), new D());
        res = eval(0xfe, new D(env));
        assume(res, 0x33);

        // define 2 // (d 0xfe (q 0x33)) ; (v 0xfe)
        eval(cons(0x07, cons(0xfe, cons(cons(0x06, cons(0x33, nil)), nil))), new D());
        res = eval(0xfe, new D(env));
        assume(res, 0x33);

        // eval 1  // (v (q 0xab))[ab:=cd]
        res = eval(cons(0x05, cons(cons(0x06, cons(0xab, nil)), nil)), new D{{0xab, 0xcd}}); 
        assume(res, 0xcd);

        // primitives
        res = eval(cons(0xab, cons(0xcd, nil)), new D {{ 0xab, (object x) => car(x) }});
        assume(res, 0xcd);

        object func;

        // functions 1  // ((q ((x) x)) 0x66)
        func = cons(cons(0x77, nil), cons(0x77, nil));
        res = eval(cons(cons(0x06, cons(func, nil)), cons(0x66, nil)), new D());
        assume(res, 0x66);

        // functions 2  // ((q ((x) (q x))) 0x66)
        func = cons(cons(0x77, nil), cons(cons(0x06, cons(0x77, nil)), nil));
        res = eval(cons(cons(0x06, cons(func, nil)), cons(0x66, nil)), new D());
        assume(res, 0x77);

        // functions 3  // ((q ((x y) y)) 0xcc 0xdd)
        var args = cons(0x76, cons(0x77, nil));
        func = cons(args, cons(0x77, nil));
        res = eval(cons(cons(0x06, cons(func, nil)), cons(0xcc, cons(0xdd, nil))), new D());
        assume(res, 0xdd);

        // define + function
        // (d 0xfe (q ((x) x)))
        func = cons(cons(0x77, nil), cons(0x77, nil));
        eval(cons(0x07, cons(0xfe, cons(cons(0x06, cons(func, nil)), nil))), new D());
        res = eval(cons(0xfe, cons(0xcc, nil)), new D(env));
        assume(res, 0xcc);

        // let 1
        res = eval(cons(0x09, cons(0xab, cons(0xcd, cons(0xab, nil)))), new D());
        assume(res, 0xcd);

        // let 2
        res = eval(cons(0x09, cons(0xab, cons(cons(0x06, cons(0xcd, nil)), cons(0xab, nil)))), new D());
        assume(res, 0xcd);


    }

    bool deep_eq(object a, object b) => (a, b) switch {
            (int ia, int ib) => ia == ib,
            (bool ba, bool bb) => ba == bb,
            (L la, L lb) => la.Zip(lb).All(ab => deep_eq(ab.First, ab.Second)),
            (_, _) => false,
        };

    void assume(object a, object b) {
        if (!deep_eq(a, b))
            throw new ArgumentException("test failed!");
    }

*/

#endif

}