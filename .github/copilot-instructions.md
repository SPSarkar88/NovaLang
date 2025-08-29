<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# NovaLang - JavaScript-like Functional Language Interpreter

This workspace contains the implementation of **NovaLang**, a functional, interpreted programming language built in **C#/.NET** with JavaScript-like syntax and developer experience.

## ✅ **Milestone 1 (M1) - Parser MVP COMPLETED**

We have successfully implemented:

- **✅ Lexer (`src/Lexer/`)**: Complete tokenization with 45+ token types including keywords, operators, literals, and punctuation
- **✅ Parser (`src/Parser/`)**: Full recursive descent parser supporting all language constructs
- **✅ AST (`src/AST/`)**: Comprehensive Abstract Syntax Tree with expressions, statements, and visitor patterns
- **✅ Error Handling**: Parse exceptions with source location information
- **✅ Testing**: Working language tests demonstrating lexer and parser functionality

## 🔄 **Current Status: Ready for Milestone 2 (M2) - Evaluator Core**

The next milestone involves implementing:
- **Environment**: Lexical scoping and variable binding
- **Values**: Runtime value system (numbers, strings, booleans, objects, functions)
- **Evaluator**: AST interpreter with visitor pattern
- **Control Flow**: Execution of if/else, loops, function calls
- **Functions**: Closures and first-class function support

## Project Structure

### Core Language Components
- `src/Lexer/`: Tokenization (Lexer.cs, Token.cs) ✅
- `src/Parser/`: Recursive descent parser (Parser.cs) ✅  
- `src/AST/`: AST node definitions (AstNode.cs, Expressions.cs, Statements.cs) ✅
- `src/Evaluator/`: AST interpreter ⏳ **NEXT**
- `src/Runtime/`: Standard library and built-ins ⏳
- `src/LanguageTest.cs`: Implementation tests ✅

### Language Design Principles

**NovaLang** follows these core principles:
- **Functional-first**: Immutability by default with opt-in mutation
- **JavaScript-like syntax**: Familiar syntax but with functional semantics
- **No OOP**: No classes, inheritance, or interfaces
- **Lexical scoping**: Proper closure support
- **Structural equality**: Value-based equality for immutable data
- **Sandboxed execution**: Safe embedding in .NET applications

### Token Types Supported (45+ types)
- **Literals**: Number, String, Boolean, Null, Undefined, TemplateString
- **Keywords**: let, const, function, return, if, else, for, while, switch, try, catch, etc.
- **Operators**: +, -, *, /, %, **, ==, ===, !=, !==, <, <=, >, >=, &&, ||, !, ??
- **Punctuation**: (, ), {, }, [, ], ,, ;, :, ., ?, =>, ...

### AST Node Types Supported (25+ types)

**Expressions:**
- LiteralExpression, IdentifierExpression, BinaryExpression, UnaryExpression
- CallExpression, MemberExpression, ArrayExpression, ObjectExpression  
- FunctionExpression, ArrowFunctionExpression, ConditionalExpression
- AssignmentExpression, LogicalExpression

**Statements:**
- ExpressionStatement, VariableDeclaration, FunctionDeclaration, BlockStatement
- IfStatement, WhileStatement, ForStatement, ReturnStatement
- BreakStatement, ContinueStatement, SwitchStatement, TryStatement, ThrowStatement

### Development Commands

```bash
# Test the current implementation
dotnet run test

# Build the project
dotnet build

# Future commands (when evaluator is complete)
dotnet run repl          # Interactive REPL
dotnet run run file.sf   # Execute script files
dotnet run fmt file.sf   # Format code
dotnet run lint file.sf  # Lint code
```

### Implementation Quality

- **🎯 Working lexer**: Processes all NovaLang syntax correctly
- **🎯 Working parser**: Handles complex nested expressions and statements
- **🎯 Comprehensive AST**: Full node coverage for language features
- **🎯 Error handling**: Parse errors with source location information
- **🎯 Test coverage**: Validated with real code examples

### Next Steps (M2 - Evaluator Core)

1. **Values System**: Runtime representation of NovaLang values
2. **Environment**: Lexical scoping with variable binding
3. **Evaluator**: AST visitor that executes the program
4. **Built-in Functions**: Console.log, basic Math operations
5. **Control Flow**: if/else, loops, function calls, returns

### Language Sample (Currently Parsed Successfully)

```javascript
// All of this parses correctly into AST
let x = 42;
const message = "Hello NovaLang!";

function add(a, b) {
    return a + b;
}

const multiply = (x, y) => x * y;

if (x > 0) {
    let result = add(x, 10);
    console.log(result);
}

const user = { name: "Ada", age: 30 };
const numbers = [1, 2, 3, 4, 5];
```

The parser successfully creates a complete AST for all these constructs, ready for evaluation!
