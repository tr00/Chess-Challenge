using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;

#nullable disable warnings

/**
 * Language Specification: (https://codegolf.stackexchange.com/questions/62886/tiny-lisp-tiny-interpreter)
 * 
 * c    "cons"      takes two arguments, a value and a list,
 *                  and returns a new list obtained by adding the
 *                  value at the front of the list.
 * 
 * h    "head"      takes a list and returns the first item in it,
 *                  or nil if given the empty list.
 * 
 * t    "tail"      takes a list and returns a new list containing 
 *                  all but the first item, or nil if given nil.
 * 
 * e    "equals"    takes two values of the same type and returns 1
 *                  if they are equal and 0 otherwise.
 * 
 * v    "eval"      takes one value and evaluates it. 
 *                  symbols get resolved, other atoms stay the same.
 *                  lists get interpreted as function calls.
 * 
 * q    "quote"     takes one expression and returns it unevaluated.
 * 
 * i    "if"        takes three expressions: a condition, 
 *                  a 'then' expression and an 'else' expression.
 *                  evaluates the condition first. if the result
 *                  is 0 or nil it evaluates & returns the 'else',
 *                  otherwise it evaluates & returns the 'then'.
 *          
 * d    "define"    takes a symbol and an expression. evaluates the
 *                  expression and binds the result to the given symbol
 *                  at the global scope. redefinition is undefined behaviour.
 *
 */
class TinyLisp {

    record Cons(object car, object cdr);

    static object env, nil;

    static void init() {
        nil = null;

        env = (new object [] { 
            nil,
            cons("c", (object x) => cons(car(x), car(cdr(x)))), 
            cons("h", (object x) => car(car(x))),
            cons("t", (object x) => cdr(car(x))),
            cons("e", (object x) => car(x) == car(cdr(x))),
            cons("nil", nil),
        }).Aggregate((x, y) => cons(y, x));

    }

    static object cons(object car, object cdr) => new Cons(car, cdr);

    static object car(object x) => ((Cons)x).car;
    static object cdr(object x) => ((Cons)x).cdr;

    static object evlis(object x, object e) {
        return x switch {
            null => nil,
            Cons (var car, var cdr) => cons(eval(car, e), evlis(cdr, e)),
            _ => nil,
        };
    }

    static object eval(object x, object e) {
        while (true) {

            if (x is string) {

                for (; e != nil; e = cdr(e))
                    if (car(car(e)) == x)
                        return cdr(car(e));


                return x; // must be a builtin macro

            } else if (x is Cons) {
                object f = eval(car(x), e);
                object a = cdr(x);

                // Console.WriteLine($"f: {f}");

                switch (f) {
                    case string s: switch (s) {
                        case "i": return eval(eval(car(a), e) != nil ? car(cdr(a)) : car(cdr(cdr(a))), e);
                        case "v": eval(car(a), e); continue;
                        case "q": return a;
                        case "d": env = cons(cons(car(a), eval(cdr(a), e)), env); return car(a);
                        default: return nil;
                    }

                    case Func<Object, Object> g: return g(evlis(a, e));

                    case Cons _: {
                        (object p, object b, a) = car(f) != null ? (car(f), cdr(f), evlis(a, e)) : (car(cdr(f)), cdr(cdr(f)), a);
                        
                        object l = e;

                        for (; p != null && a != null; p = cdr(p), a = cdr(a))
                            l = cons(cons(car(p), car(a)), l);

                        x = b;
                        e = l;

                        continue;
                    }

                    default: return nil;
                }

            } else {
                return x;
            }
        }
    }

    static object print(object x, bool head_of_list = true) {
        switch (x) {
            case null: Console.Write("nil"); break;
            case Cons (var car, var cdr): {
                if (head_of_list) 
                    Console.Write("(");
                
                print(car, true);

                if (cdr != null) {
                    Console.Write(" ");
                    print(cdr, false);

                    if (!(cdr is Cons))
                        Console.Write(")");
                } else {
                    Console.Write(")");
                }
                break;
            }
            default: Console.Write(x); break;
        }

        return x;
    }

    public static void Main() {

        // Console.WriteLine(rt.env);

        // var tmp = rt.eval(rt.cons("c", rt.cons(1, rt.cons(2, rt.nil))), rt.env);

        // Console.WriteLine(tmp);

        // var tmp = rt.cons("q", rt.cons(1, rt.cons(2, rt.cons(3, rt.nil))));

        // var res = rt.eval(rt.cons("t", rt.cons(tmp, rt.nil)), rt.env);

        // rt.print(res);

        init();

        object cond_expr = cons("q", "tru"); // "nil"; // "tru";
        object then_expr = cons("q", "then...");
        object else_expr = cons("q", "else...");

        object expr = cons("i", cons(cond_expr, cons(then_expr, cons(else_expr, nil))));


        print(eval(expr, env));

        Console.WriteLine();

        Console.WriteLine("running tests...");

        Test();

        Console.WriteLine("tests completed.");

    }

    // ...eat your vegetables...
    public static void Test() {

        object x, r; 

        x = "nil";
        r = nil;

        assert(x, r);

        x = "lululu";
        r = "lululu";

        assert(x, r);

        x = cons("q", "nil");
        r = "nil";

        assert(x, r);

        x = cons("q", "lululu");
        r = "lululu";

        assert(x, r);

        x = cons("q", cons("lulu", "lolo"));
        r = cons("lulu", "lolo");

        assert(x, r);

        x = cons("q", cons("quop", nil));
        r = cons("quop", nil);

        assert(x, r);

        x = cons("i", cons("tru", cons(cons("q", "then..."), cons(cons("q", "else..."), nil))));
        r = "then...";

        assert(x, r);

        x = cons("i", cons("nil", cons(cons("q", "then..."), cons(cons("q", "else..."), nil))));
        r = "else...";

        assert(x, r);

        x = cons("i", cons(cons("q", "tru"), cons(cons("q", "then..."), cons(cons("q", "else..."), nil))));
        r = "then...";

        assert(x, r);



    }

    static bool cmp(object a, object b) {
        return (a, b) switch {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,

            (string sa, string sb) => sa == sb,
            (Cons _, Cons _) => cmp(car(a), car(b)) && cmp(cdr(a), cdr(b)),

            (_, _) => false, // I don't properly support anything else (I think)
        };
    }

    static void assert(object x, object r) {
        init();

        object q = eval(x, env);

        if (cmp(q, r)) return;

        Console.WriteLine("test failed!");
        Console.Write("expression: ");
        print(x);

        Console.Write("\nevaluation: ");
        print(q);

        Console.Write("\nexpectation: ");
        print(r);

        Console.WriteLine("\n");
    }

}

#nullable restore warnings
