
// #define VEGETABLES
// #define DEBUGINFO

using ChessChallenge.API;

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using L = System.Collections.Generic.IEnumerable<object>;
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

    public MyBot() {

        /* core library */
        env = new() {
            { 0x0a, (object x) => cons(car(x), car(cdr(x))) },
            { 0x0b, (object x) => car(car(x)) },
            { 0x0c, (object x) => cdr(car(x)) },
            { 0x0d, (object x) => (object)nilq(car(x)) },
            // TODO: eq

            { 0x10, (object x) => (object)((Board)car(x)).GetLegalMoves().Cast<object>() },
        };


#if VEGETABLES
        vegetables(); // #DEBUG
#endif

        Stack<List<object>> stack = new();

        foreach (var token in (new decimal[] {

642024631332551435634147585m,
5571986344859797580280825857m,
641996298361729476644244225m,
2167608827689444598719054338m,
4953021047589239525709578751m,
37038168493095256596m,

        }).SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes))
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

        eval(stack.Peek()[0], env);
    }

    object cons(object car, object cdr) => car switch {
        L l => l.Prepend(car),
        _ => new object[] { car, cdr },
    };

    object car(object x) => ((L)x).First();

    object cdr(object x) => ((L)x).Skip(1);

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
        var args = cdr(x);

        if (func is int)
            switch ((int)func) {
                case 0x05: // eval
                    x = eval(car(args), e);
                    goto TCO;

                case 0x06: // quote
                    return car(args);

                case 0x07: // define
                    var tmp = eval(car(cdr(args)), env);
                    env[car(args)] = tmp;
                    return tmp;

                case 0x08: // if
                    x = (bool)eval(car(args), e) ? car(cdr(args)) : car(cdr(cdr(args)));
                    goto TCO;
#if DEBUGINFO
                default: throw new ArgumentException($"unrecognized symbol: {func}"); // #DEBUG
#endif
            }


        // TODO: the tolist is optional
        var iter = ((L)args).Select(arg => eval(arg, e)).ToList();

        if (func is F) 
            return ((F)func)(iter);

        e = new(e);

#if DEBUGINFO
        Console.Write("lambda: ");      // #DEBUG
        print(func);                    // #DEBUG
        Console.WriteLine();            // #DEBUG
#endif

        foreach ((var k, var v) in ((L)car(func)).Zip(iter))
            e[k] = v;

        x = car(cdr(func));

        goto TCO;
    }

    public Move Think(Board board, Timer timer) {
        Console.WriteLine("thinking..."); // #DEBUG

        return (Move)eval(new object[] { 0xff, board, timer }, env);
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
        res = eval(new object[] {}, new D());
        assume(nilq(res), true);

        // lookup
        res = eval(0xab, new D {{0xab, 0xcd}});
        assume(res, 0xcd);

        // quote 1
        res = eval(cons(0x06, 0x55), new D());
        assume(res, 0x55);

        // quote 2
        tmp = new object[] { 0xab };
        res = eval(cons(0x06, tmp), new D());
        assume(res, tmp);

        // quote 3
        tmp = new object[] { 0xfe, 0xdc };
        res = eval(cons(0x06, tmp), new D());
        assume(res, tmp);

        // if 1
        res = eval(new object[] { 0x08, true, 0xab, 0xcd }, new D());
        assume(res, 0xab);
        res = eval(new object[] { 0x08, false, 0xab, 0xcd }, new D());
        assume(res, 0xcd);

        // if 2
        res = eval(new object[] { 0x08, true, cons(0x06, 0xab), 0xcd }, new D());
        assume(res, 0xab);
        res = eval(new object[] { 0x08, false, 0xab, cons(0x06, 0xcd) }, new D());
        assume(res, 0xcd);

        // if 3
        res = eval(new object[] { 0x08, cons(0x06, true), 0xab, 0xcd }, new D());
        assume(res, 0xab);
        res = eval(new object[] { 0x08, cons(0x06, false), 0xab, 0xcd }, new D());
        assume(res, 0xcd);

        // define 1
        eval(new object[] {0x07, 0xfe, 0x33}, new D());
        res = eval(0xfe, new D(env));
        assume(res, 0x33);

        // define 2
        eval(new object[] {0x07, 0xfe, cons(0x06, 0x33)}, new D());
        res = eval(0xfe, new D(env));
        assume(res, 0x33);

        // eval 1
        res = eval(cons(0x05, cons(0x06, 0xab)), new D{{0xab, 0xcd}}); // (v (q 0xab))[ab:=cd]
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