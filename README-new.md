# NovaLang

A functional, interpreted programming language built in **C#/.NET** with JavaScript-like syntax and developer experience.

## Project Status: 🚧 **In Development**

**Current Progress (Milestone 1 - Parser MVP):**
- ✅ **Lexer**: Complete tokenization of NovaLang source code
- ✅ **AST Nodes**: Full Abstract Syntax Tree node definitions
- ✅ **Parser**: Recursive descent parser for all language constructs
- ✅ **Error Handling**: Basic parse error reporting with source locations
- 🔄 **Evaluator**: In progress (Milestone 2)
- ⏳ **Standard Library**: Planned (Milestone 4)
- ⏳ **REPL**: Planned (Milestone 5)

## Language Features

**NovaLang** is a functional-first programming language that:
- 🔸 Provides JavaScript-like syntax while remaining functional-first
- 🔸 **Does NOT support OOP** (no classes/inheritance)  
- 🔸 Supports structs/records, first-class functions, variables, and common data types
- 🔸 Includes control flow (loops, switch, if/else) and error handling
- 🔸 Offers REPL and file interpreter capabilities
- 🔸 Features sandboxing and embedding support for .NET applications

### Language Syntax Example

```javascript
// Variables and structs
const rate = 0.18
let total = 0
const user = { id: 1, name: "Ada" }
const enriched = { ...user, active: true }

// Functions and closures
const add = (a, b) => a + b
function makeAdder(x) { return (y) => x + y }

// Control flow and arrays
if (total === 0) { total = add(2, 3) }
for (let i = 0; i < 3; i = i + 1) { Console.log(i) }
const nums = [1,2,3]
const doubled = nums.map(n => n * 2)

// Pattern matching and error handling
switch (user.name) {
  case "Ada": Console.log("Hi")
  default: Console.log("Hello")
}
try { throw Error("boom") } catch (e) { Console.error(e) }
```

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- Visual Studio Code (recommended)
- C# Dev Kit extension for VS Code

### Building the Project
```bash
dotnet build
```

### Testing the Implementation
```bash
# Run language implementation tests
dotnet run test

# Test with example script (when evaluator is complete)
dotnet run run example.sf
```

### Using VS Code Tasks
- **Build**: Press `Ctrl+Shift+P` and run "Tasks: Run Task" then select "build"
- **Run**: Press `Ctrl+Shift+P` and run "Tasks: Run Task" then select "run"

## CLI Commands (Coming Soon)

```bash
# Interactive REPL
jsf repl

# Execute script files  
jsf run script.sf

# Format code
jsf fmt script.sf

# Lint code
jsf lint script.sf

# Run tests
jsf test
```

## Architecture

### Implementation Structure
```
Frontend: Lexer → Parser → AST (with source maps)
Core: Evaluator (AST walker), Environment/Scope chain, Values model
Runtime: Standard library (Math, Array, String, JSON, Date, Console)
Host Bridge: Marshalling, Sandboxing, Debug hooks
Tools: Formatter, Linter, CLI, REPL
```

### Language Features (Planned)

- **Variables**: `let` (mutable) and `const` (immutable) with block scoping
- **Data Types**: 
  - Primitives: `number`, `int`, `bool`, `string`, `null`, `undefined`, `symbol`
  - Compounds: structs/records `{a: 1, b: "x"}`, arrays `[]`, maps/sets
- **Functions**: First-class functions, closures, arrow syntax `(x, y) => x + y`
- **Control Flow**: `if/else`, `switch`, `for`, `while`, `do-while`, `try/catch/finally`
- **Modules**: ES-module-like `import`/`export` syntax

## Development Milestones

- [x] **M1** - Parser MVP (Lexer, Parser, AST, Error reporting) ✅ **COMPLETE**
- [ ] **M2** - Evaluator Core (Environment, Values, Control flow, Functions) 🔄 **IN PROGRESS**
- [ ] **M3** - Data Types (Arrays, Structs, JSON, Equality semantics)
- [ ] **M4** - Standard Library (Math, String, Array methods, Console)
- [ ] **M5** - Tooling (REPL, CLI, Formatter, Linter)
- [ ] **M6** - Interop & Security (Embedding API, Sandboxing, Timeouts)
- [ ] **M7** - Documentation & Hardening (Spec, Examples, Performance, Fuzzing)

## Project Structure

```
src/
├── Lexer/          # Tokenization and lexical analysis
├── Parser/         # Recursive descent parser  
├── AST/           # Abstract Syntax Tree node definitions
├── Evaluator/     # AST interpretation and execution (coming soon)
├── Runtime/       # Standard library and built-ins (coming soon)
└── LanguageTest.cs # Implementation tests
```

## Contributing

This project is currently in active development. The language specification and implementation are being built following functional programming principles with a focus on:

- **Immutability by default** with opt-in mutation
- **Lexical scoping** and proper closure support
- **Structural equality** for immutable data structures
- **Sandboxed execution** for safe embedding

## License

This project is open source and available under the [MIT License](LICENSE).
