# Timeline of Developement

### The Idea (1 week after video)

I had the idea of abusing an interpreter very early after watching the video when it came out.
Together with a good RL friend I spent a weekend figuring out the API, 
how C# works, what features we might want in our bot etc.

At that time I thought I could just use 1 string for all the source code and it
counts as a single token, like it usually does. 
However SebLague has a special rule inplace which says that every char in a string
counts as token.

I learned this from asking on the discord. I was kinda upset after that and didn't
continue developing after that...

All this happened within the first week or two after the video.

### 2nd tryy (5 weeks until deadline)

When I gave up I knew that I could have used 64bit integer literals instead of the string.
I thought they would take up 2 tokens, one for the integer and one for the comma, per 8 bytes.
That would be a gain of 4 bytes per token which I believed, 
would not offset the cost in tokens of the interpreter.

As of this writing (~30days till deadline) I don't know whether that assumption was correct or not.
One months later, when visiting a different friend who is also into computer science, 
I showed them the video and explained the plan I had about using an interpreter and all that.

They motivated me to go ahead and do it, so we spend the next hours plotting the bot. 
I went ahead and spent almost the whole night implementing a lisp interpreter in C#.
Two languages with which I was unfamiliar with.

The days after that I spent with family & friends before going back to my wizard retreat.

### The Runtime (34 days until deadline)

At this point I have a not working lisp interpreter and I know that I have time 
until the 9th of september before I don't have that much spare time anymore. 
This gives me 1.5 weeks to write this piece of upside down ingeniuty.

I continued with lisp interpreters/runtimes for the next 3 days.
Each day I implemented a new version because of how I wanted to handle 
`cons`, `car` & `cdr` and how the underlying list works.

This may not seem like the best time management but I learned a lot about 
how lisp works internally and even more about C# and its very unhelpful type system.

I spent another day golfing & debugging the third version of the runtime.

### The Compiler (30 days until deadline)

Okay, now I got a runtime which consumes lisp in bytecode form 
but I need a compiler to produce that bytecode 
unless I want to write the bot in my own lisp flavored assembly language.

So I spent another day writing a hacky compiler which parses s-expressions
and converts them into s-expressions but compressed into bytes.

At this stage I also learned about decimal packing from skimming thru discord.
The compiler reads a string of lisp code and prints an array of decimals.

###