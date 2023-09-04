#define DEBUG_CTOR
#define DEBUG_EVAL

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
    object bot;

    public MyBot() {
        Stack<List<object>> stack = new();

        foreach (var token in (new decimal[] {

            /* bytecode */
            37040431283831046401m,

            /* decoder */
        }).SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes))

            /* parser */
            switch (token) {
                case 0x00: /* pad */
                    continue;
                case 0xff: /* nil */
                    stack.Peek().Add(null); 
                    break;
                case 0x01: /* lpa */
                    stack.Push(new()); 
                    break;
                case 0x02: /* rpa */
                    var top = stack.Pop();
                    stack.Peek().Add(top);
                    break;
                default:
#if DEBUG_CTOR
                    debug("[ctor]> decoded symbol: ", (int)token); // #DEBUG
#endif
    
                    stack.Peek().Add((int)token);
                    break;
            }

        /* core library */
        env = new() {
            { 0x08, (object x) => car(car(x)) },
            { 0x0c, (F)((object x) => ((Board)car(x)).GetLegalMoves().Cast<object>()) },
        };

        var ast = stack.Peek()[0];

#if DEBUG_CTOR
        debug("[ctor]> decoded ast: ", ast); // #DEBUG
#endif

        /* initialization */
        bot = eval(ast, env);

#if DEBUG_CTOR
        Console.WriteLine("[ctor]> initialization completed...\n");
#endif
    }

    object cons(object car, object cdr) => cdr switch {
        L l => l.Prepend(car),
        _ => new object[] { car, cdr },
    };

    object car(object x) => ((L)x).First();

    object cdr(object x) => ((L)x).Skip(1);

    bool nilq(object x) => x switch {
        L l => !l.Any(),
        _ => false,
    };

    object eval(object x, D e) {
        TCO: // tail call optimization

#if DEBUG_EVAL
        debug("[eval]> starting evaluation of: ", x); // #DEBUG
#endif

        if (x is not L)
        { // #DEBUG
#if DEBUG_EVAL
            Console.WriteLine("[eval]> env[{0}] = {1}", x, e.ContainsKey(x) ? "found." : "missing!"); // #DEBUG
#endif

            return e.ContainsKey(x) ? e[x] : x;
        } // #DEBUG

        if (nilq(x))
            return x;

        var func = eval(car(x), e);
        var args = cdr(x);

        if (func is int)
            switch ((int)func) {
                case 0x03: x = args; goto TCO;
                case 0x04: return args;
                case 0x05: env[car(args)] = eval(car(cdr(args)), env); return car(args);
                case 0x06: return eval(nilq(eval(car(args), e)) ? car(cdr(args)) : car(cdr(cdr(args))), e);
            }

        var iter = ((L)args).Select(arg => eval(arg, e));

        if (func is F) 
        { // #DEBUG
            return ((F)func)(iter);
        } // #DEBUG

        e = new(e);

        debug("calling: ", func); // #DEBUG

        foreach ((var k, var v) in ((L)car(func)).Zip(iter))
            e[k] = v;

        x = car(cdr(func));

        goto TCO;
    }

    public Move Think(Board board, Timer timer) {
        debug("thinking... ", bot); // #DEBUG

        env[0x0a] = board;
        env[0x0b] = timer;

        return (Move)eval(bot, env);
    }

    // debugging:

void debug(string prefix, object expr) { Console.Write(prefix); print(expr); Console.WriteLine(); } // #DEBUG

    void print(object x) {                                  // #DEBUG 
        switch (x) {                                        // #DEBUG
            case null: Console.WriteLine("nil"); break;     // #DEBUG
            case L a: {                                     // #DEBUG
                Console.Write("(");                         // #DEBUG
                if (!nilq(a))
                    print(car(a));                          // #DEBUG

                a = (L)cdr(a);                              // #DEBUG
                
                while (!nilq(a)) {                          // #DEBUG
                    Console.Write(" ");                     // #DEBUG
                    print(car(a));                          // #DEBUG
                    a = (L)cdr(a);                          // #DEBUG
                }                                           // #DEBUG
                Console.Write(")"); break;                  // #DEBUG
                }                                           // #DEBUG
            case byte: Console.Write("0x{0:x2}", x); break; // #DEBUG
            default: Console.Write(x); break;               // #DEBUG
        }                                                   // #DEBUG
    }                                                       // #DEBUG

}