# Tiny Lisp Chess Bot 

This is my submission to SebLague's [Chess Challenge](https://github.com/SebLague/Chess-Challenge). 
My primary focus was to get around the token limit rather than implementing the best possible bot.
I might write a more in-depths explanation in the future, but here is the basic gist.


If I can write my entire bot inside of a string and use the remaining 1023 tokens to
implement an interpreter for that string, than I can basically make my bot as complex as I want to...
However, strings don't count as 1 token, as they usually do, instead each char counts as token.

Okay, next attempt: Let's store the string as an array of integer literals instead. 
Comma's are not counted so we can pack 8 bytes into 1 token. 
If our language uses each byte as a single token we can basically 8x the token limit
minus the tokens needed for the interpreter.

```
"TinyLisp"
=> ['T', 'i', 'n', 'y', 'L', 'i', 's', 'p']
=> [0x54, 0x69, 0x6e, 0x79, 0x4c, 0x69, 0x73, 0x70]
=> 0x54696e794c697370
```

Now I just have to create a small but expressive language and make it's interpreter fit into the 1024 tokens.
Here you can see a simplified sketch of `MyBot.cs`. 
Feel free to dive into the actual code in this repo as well, 
but there it's much less understandable because of token optimization and lots of trickery.

```C#

code_t code;

public MyBot() {
    var packed_code = new ulong[] {
        0x796f75747562652e,
        0x636f6d2f77617463,
        0x683f763d70375958,
        0x5869656768746f00,
        // ...
    };

    var unpacked_code = code.SelectMany(BitCoverter.GetBytes);

    code = parse(unpacked_code);
}

public Move Think(Board board, Timer, timer) {
    return (Move) interpret(code);
}

code_t parse() { /* ... */ }

void interpret(code_t code) { /* ... */ }

```

Because the bot is interpreted it's speed is gonna be - well - slow, which is an issue for chess bots but idc.

## Tiny Lisp Language Specification

I need a language which requires very little tokens to implement, which is expressive enough to be useful and 
can be compressed into a '1 byte = 1 token' representation.

I've chosen lisp because it is arguably the most expressive language in the entire universe and it's easy to implement.
Using macros for common expressions I can further shrink the code size if needed.

I've used a modified version of [TinyLisp](https://github.com/dloscutoff/Esolangs/tree/master/tinylisp) so credit goes to them for coming up with the language.
My Implementation is roughly based on the one described [here](https://codegolf.stackexchange.com/a/62975).

### Builtins

- `(define name expr)` binds name in the global scope to the evaluation of expr and returns that evaluation. 
    This differs slightly from the original which returns the name.

- `(eval expr)`: Evaluates `expr`. Who would have thought...

- `(quote expr)`: Return the `expr` without evaluating it. This is useful to work with macros and also to define data structures.

- `(if cond then else)`: First evaluates `cond` and the either evaluates `then` or `else` depending on the evaluation of `cond`.

- `(let name expr cont)`: Binds `name` in the current scope to the evaluation of `expr` and then continues to evaluate `cont`.
    This builtin was not part of the original tinylisp but I felt like I need it.


### Bytecode

The bytecode is basically a 1-to-1 mapping of lisp syntax but to bytes: (I just assume you know how lisp's syntax works)

- `0x00` is ignored when parsing
- `0x01` represents `)`
- `0x02` represents `)`
- everything else is a symbol
- `0xff` is the `_start` symbol which is the entry point used by the `Think()` method.

Because there is only place for 256 - 3 unique symbols, they are more like register names in a register file than dynamically scoped symbols.
To see how I used which bytes/symbols/registers see the registers.txt file in this repo.

The padding byte `0x00` exists so I can pad the code size to a mutliple of the packing literal length (8 in the example above).




