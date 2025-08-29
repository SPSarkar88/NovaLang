# NovaLang Test Suite

This directory contains comprehensive tests for the NovaLang interpreter, organized into different categories:

## Test Structure

### Unit Tests (`unit/`)
- **LexerTests.cs** - Tests for tokenization and lexical analysis
- **ParserTests.cs** - Tests for AST generation and syntax parsing
- **ValueTests.cs** - Tests for runtime value system and operations
- **EnvironmentTests.cs** - Tests for variable scoping and environment management

### Integration Tests (`integration/`)
- **BasicLanguageTests.cs** - End-to-end tests for core language features
- **M3FeatureTests.cs** - Tests for Milestone 3 features (spread, destructuring, templates)
- **BuiltinFunctionTests.cs** - Tests for built-in functions and standard library
- **ScriptTests.cs** - Tests that execute complete script files

### Script Tests (`scripts/`)
Complete NovaLang programs that test real-world usage:
- **arithmetic.sf** - Basic arithmetic and variable operations
- **functions.sf** - Function declarations, calls, and arrow functions  
- **arrays_objects.sf** - Array and object manipulation
- **control_flow.sf** - If/else statements, loops, and control structures
- **m3_features.sf** - Spread syntax, destructuring, and template literals
- **builtins.sf** - Built-in functions, Math operations, and type checking

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run with Detailed Output
```bash
dotnet test --verbosity normal
```

### Run Specific Test Categories
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only  
dotnet test --filter "FullyQualifiedName~Integration"

# Specific test class
dotnet test --filter "ClassName=M3FeatureTests"
```

### Run Individual Test Methods
```bash
dotnet test --filter "TestArraySpreadSyntax"
```

## Test Coverage

The test suite covers:

### ✅ **Core Language Features**
- Variable declarations (let, const)
- All data types (numbers, strings, booleans, null, undefined, arrays, objects)
- Arithmetic and logical operations
- Function declarations and arrow functions
- Control flow (if/else, while, for loops)
- Lexical scoping and closures

### ✅ **M3 Advanced Features**
- Spread syntax for arrays: `[...arr, newItem]`
- Spread syntax for objects: `{...obj, newProp: value}`
- Array destructuring: `let [a, b, ...rest] = array`
- Object destructuring: `let {prop1, prop2: alias} = obj`
- Basic template literals: `` `hello world` ``

### ✅ **Built-in Functions**
- **Console**: `console.log()`, `console.error()`, `console.warn()`
- **Type checking**: `typeof` operator
- **Math functions**: `abs`, `floor`, `ceil`, `round`, `sqrt`, `pow`, `min`, `max`, `random`
- **Array utilities**: `Array.isArray()`

### ✅ **Error Handling**
- Parse errors with source location information
- Runtime errors with proper exception handling
- Undefined variable detection
- Type checking and validation

## Expected Test Results

All tests should pass when the NovaLang interpreter is functioning correctly. The test suite includes:

- **~15 Unit test classes** testing individual components
- **~50 Integration tests** testing feature combinations  
- **6 Script tests** executing complete programs
- **~100+ Total test cases** covering all implemented features

## Test Development

When adding new features to NovaLang:

1. **Add Unit Tests** - Test the individual component (lexer, parser, evaluator)
2. **Add Integration Tests** - Test the feature end-to-end
3. **Add Script Tests** - Create a complete program demonstrating the feature
4. **Update Documentation** - Document expected behavior and usage

The test suite serves as both validation and documentation for NovaLang's capabilities.
