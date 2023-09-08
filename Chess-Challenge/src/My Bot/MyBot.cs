
// #define VEGETABLES
// #define DEBUGINFO

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
        // 0x17 => a / b,
        0x18 => a == b,
        0x19 => a >= b,
        0x1a => a < b,

    };
    

    public MyBot() {
        nil = new L();

        /* core library */
        env = new D {
            { 0x0a, (object x) => cons(car(x), car(cdr(x))) },
            { 0x0b, (object x) => car(car(x)) },
            { 0x0c, (object x) => cdr(car(x)) },
            { 0x0d, (object x) => (object)nilq(car(x)) },
            { 0x0e, (object x) => { print(car(x));Console.WriteLine(); return car(x); }},


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
            { 0x18, binop_factory(0x18) },
            { 0x19, binop_factory(0x19) },
            { 0x1a, binop_factory(0x1a) },

            { 0x20, (object x) => (object)((Board)car(x)).GetLegalMoves().Cast<object>().ToList() },
            { 0x21, (object x) => {
                var board = (Board)car(x);
                return board.GetAllPieceLists()
                    .Select((PieceList x) => (object)x.Cast<object>().ToList()).ToList(); } },
            { 0x22, (object x) => (object)((Board)car(x)).IsWhiteToMove },
            { 0x23, (object x) => {
                var board = (Board)cxr(x, 0);
                var move  = (Move)cxr(x, 1);
                board.MakeMove(move);
                return (object)board;
            }}, 
            { 0x24, (object x) => {
                var board = (Board)cxr(x, 0);
                var move  = (Move)cxr(x, 1);
                board.UndoMove(move);
                return (object)board;
            }}, 
        };


#if VEGETABLES
        vegetables(); // #DEBUG
#endif

        Stack<L> stack = new();

        var code = new [] {

620183778591734146619474177m,
626251927926882121323849217m,
621397337274691388933474817m,
12390285484634985770009100801m,
13323576327985931597826303233m,
13048125284362982253807086082m,
362720265774158998607170561m,
670963293797665053235282213m,
319161178737031824775119106m,
13322368084700044355275205389m,
929692311246474835600212482m,
15423315101741393051454133498m,
78181191172478126049887821303m,
932114801083904262255346438m,
15351837362656678101499631386m,
1046971341846453811746217463m,
1234280267665511217879055609m,
63805376446696234458320611880m,
1046971341693332497923006984m,
924795257844166403094215929m,
2477099356472887205842977026m,
363891799976544134681857281m,
366309282761123217891536907m,
311912343231760279439290380m,
366309651610761755191489033m,
5263904486556330787426415393m,
372391672331076032254711041m,
4024551851035646046200070437m,
2786584437415099613826122293m,
312172979656407798093513018m,
17356552755362838850072678421m,
312163663330018559061525009m,
312172979728411516715270411m,
312172979728457695313920514m,
16782318746432833779046429964m,
18270500671240542080078460674m,
7428886959070441802135840001m,
9904971040096651847019925560m,
382063078887993298220614195m,
319161180049569660156444965m,
312153349886462797709194509m,
688120277776058718395382281m,
6500404574083840380144456961m,
685735298220413526206452024m,
688120296209010711335078657m,
688120296223084206633386497m,
17952823316531217384512752641m,
17643338306709872316694069814m,
384480930527222323749192253m,
680776374309059031297360165m,
13423928885127215564201434115m,
311912343174985815462906062m,
990403051594967477414657801m,
620188427306898040695149725m,
621397353053053628184658434m,

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

    object cxr(object x, int i) => ((L)x)[i];

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
        var res = eval(new L{0xff, board, timer}, env);
        // print(res);
        return (Move)res;
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