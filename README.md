# ScratchScript

ScratchScript is a javascript-like programming language that compiles to Scratch projects (.sb3).

It uses ANTLR4 for tokenizing/parsing and has an IR language that converts assembly-like commands to Scratch blocks.

[Wiki/Documentation](https://scratchscript.github.io) (outdated)

# State

ScratchScript is currently being rewritten to have unit tests, proper project structure & common sense. The list of implemented features can be found below:

- [ ] Frontend
  - [ ] Enums
    - [ ] Declaration
    - [ ] Usage
    - [ ] `enum.value` and `enum.name`
    - [ ] Export
  - [ ] Variables
    - [ ] Declaration
    - [ ] Assignment
    - [ ] Function arguments
    - [ ] `const` & exporting `const`s
  - [ ] Lists
    - [ ] Declaration of 1D lists
    - [ ] Access of 1D lists
    - [ ] Declaration of multidimensional lists
    - [ ] Access of multidimensional lists
    - [ ] Passing a list as a function argument
    - [ ] Initializing variables with lists (list expressions)
  - [ ] Expressions
    - [ ] Constant expressions
    - [ ] Identifier expressions
    - [ ] Interpolated string expressions
    - [ ] Binary expressions
        - [ ] Binary add expressions (`+`, `-`, `*`, `**`, `%`, `/`)
        - [ ] Binary compare expressions (`>`, `>=`, `<`, `<=`, `==`, `!=`)
        - [ ] Binary shift expressions (`>>`, `<<`)
        - [ ] Binary bitwise expressions (`&`, `|`, `^`)
    - [ ] Unary expressions (`+`, `-`, `!`)
    - [ ] Function call expressions (`a()`)
    - [ ] Member function call expressions (`a.b()`)
    - [ ] Member property access expressions (`a.b`)
    - [ ] Ternary expressions (`a ? b: c`)
  - [ ] Attributes
    - [ ] `@inline`
    - [ ] `@noexport`/`@private`
    - [ ] `@import` (asset importing)
    - [ ] `@unicode`
      - [ ] `std/string/unicode` functions (extended string handling)
  - [ ] Functions
    - [ ] Declaration
    - [ ] Call
    - [ ] Export (a.k.a. dependencies handling)
  - [ ] Control flow
    - [ ] While loop
    - [ ] For loop
    - [ ] Foreach loop
    - [ ] Repeat loop
    - [ ] Breaking out of loops
    - [ ] If/else if/else statements
    - [ ] Switch statements
  - [ ] Imports
    - [ ] Handling name collisions
    - [ ] Importing specific items instead of all
    - [ ] Namespaces & namespace aliases (`import * as math from 'std/math'`)
  - [ ] Events
    - [ ] `start`
    - [ ] `stop`
    - [ ] Built-in Scratch events
    - [ ] Custom events (broadcasts)
  - [ ] Other
    - [ ] `throw`
    - [ ] `debugger`
    

- [ ] Backend
  - Everything. I haven't started implementing it yet.