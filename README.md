# ScratchScript

ScratchScript is a typescript-like programming language that compiles to Scratch projects (.sb3).

It uses ANTLR4 for tokenizing/parsing and has an IR language that converts assembly-like commands to Scratch blocks.

[Wiki/Documentation](https://scratchscript.github.io) (outdated)

# Code sample

```ts
import 'scratch/looks';

function add(x: number, y: number) {
    // return values in functions:
    return x + y;
}

on start {
    sayTimed('hello world!', 1);
    sayTimed('ScratchScript has all kinds of features that native Scratch lacks, like:', 5);
    sayTimed(`string interpolation: ${2 + 2} <- should be 4`, 3);
    sayTimed(`nested function calls: ${add(3, add(5, 2))} <- should be 10`, 3);
    
    // for loops:
    for(let i = 0; i < 10; i++) {
        // ...
    }
    
    // ternary operator:
    let theory = 2 + 2 == 4 ? "correct": "incorrect";
    
    // exponents and bitwise operations:
    let num = (2 ** 3) >> 3;
    
    // ...and much more!
}   
```

# State

ScratchScript is currently being rewritten to have unit tests, proper project structure & common sense.

<details>
  <summary>Implemented features</summary>

  - [ ] Frontend
    - [ ] Enums
      - [x] Declaration
      - [ ] Usage
      - [ ] `enum.value` and `enum.name`
      - [x] Export
    - [ ] Variables
      - [x] Declaration
      - [x] Assignment
      - [x] Function arguments
      - [ ] `const` & exporting `const`s
    - [ ] Lists
      - [ ] Declaration of 1D lists
      - [ ] Access of 1D lists
      - [ ] Declaration of multidimensional lists
      - [ ] Access of multidimensional lists
      - [ ] Passing a list as a function argument
      - [ ] Initializing variables with lists (list expressions)
    - [ ] Expressions
      - [x] Constant expressions
      - [x] Identifier expressions
      - [ ] Interpolated string expressions
      - [x] Binary expressions
          - [x] Binary add expressions (`+`, `-`, `*`, `**`, `%`, `/`)
          - [x] Binary compare expressions (`>`, `>=`, `<`, `<=`, `==`, `!=`)
          - [ ] Binary shift expressions (`>>`, `<<`)
          - [ ] Binary bitwise expressions (`&`, `|`, `^`)
      - [x] Unary expressions (`+`, `-`, `!`)
      - [ ] Function call expressions (`a()`)
      - [ ] Member function call expressions (`a.b()`)
      - [ ] Member property access expressions (`a.b`)
      - [ ] Ternary expressions (`a ? b: c`)
    - [ ] Attributes
      - [ ] `@inline`
      - [ ] `@import` (asset importing)
      - [ ] `@unicode`
    - [ ] Functions
      - [x] Declaration
      - [ ] Call
      - [ ] Export (a.k.a. dependencies handling)
    - [ ] Control flow
      - [x] While loop
      - [x] For loop
      - [ ] Foreach loop
      - [x] Repeat loop
      - [ ] Breaking out of loops
      - [x] If/else if/else statements
      - [ ] Switch statements
    - [ ] Imports
      - [ ] Handling name collisions
      - [ ] Importing specific items instead of all
      - [ ] Namespaces & namespace aliases (`import * as math from 'std/math'`)
    - [ ] Events
      - [x] `start`
      - [ ] `stop`
      - [ ] Built-in Scratch events
      - [ ] Custom events (broadcasts)
    - [ ] Other
      - [ ] `throw`
      - [ ] `debugger`
    - [ ] `std` library
      - [ ] `std/native` (a.k.a the `scratch` namespace)
        - [x] `scratch/control`
        - [ ] `scratch/data`
        - [ ] `scratch/looks`
        - [ ] `scratch/motion`
        - [ ] `scratch/operators`
        - [ ] `scratch/sensing`
        - [ ] `scratch/sound`
      - [ ] `std/string`
        - [ ] `std/string/unicode`
      - [ ] `std/math`
      
  
  - [ ] Backend
    - Nothing. I haven't started implementing it yet.

</details>