# NovaLang v1.0.0-alpha Release

**🎉 First Production-Ready Release of NovaLang - JavaScript-like Functional Programming Language**

## 🚀 Major Features

### Complete Language Implementation
- ✅ **Lexer & Parser**: 45+ token types, 25+ AST nodes with full JavaScript-like syntax
- ✅ **Evaluator**: Tree-walking interpreter with lexical scoping and closures
- ✅ **Standalone Executable**: Self-contained distribution, no .NET runtime required
- ✅ **Interactive REPL**: Live development environment

### M3 Advanced Features - All Production Ready
- ✅ **Template Literals**: Full interpolation `Hello ${name}!`
- ✅ **Spread Syntax**: Arrays `[...arr]` and Objects `{...obj}` 
- ✅ **Destructuring**: `let [a, b, ...rest] = array` and `let {name, age} = obj`
- ✅ **I/O System**: `console.log()`, `print()`, and `input()` functions

### Developer Experience  
- ✅ **5 Essential Examples**: 16.6KB of curated tutorials and demos
- ✅ **Complete Documentation**: Comprehensive README with examples
- ✅ **Multiple Test Types**: Built-in tests, script tests, and integration tests
- ✅ **Clean CLI**: `novalang script.sf`, `novalang repl`, `novalang test`

## 🎯 What You Can Do

```javascript
// Template literals with expressions
let name = "Alice";
let greeting = `Hello ${name}! Today is ${new Date()}`;

// Spread syntax for arrays and objects
let skills = ["JavaScript", "C#"];
let allSkills = [...skills, "NovaLang", "Functional Programming"];
let user = {name: "Bob", age: 30};
let profile = {...user, skills: allSkills, active: true};

// Destructuring assignment
let [primary, ...others] = allSkills;
let {name: userName, age: userAge} = profile;

// Interactive I/O
let input_name = input("What's your name? ");
print(`Welcome ${input_name}!`);

// Functions and closures
const makeCounter = () => {
    let count = 0;
    return () => ++count;
};
```

## 📦 Getting Started

```bash
# Clone and build
git clone https://github.com/SPSarkar88/NovaLang.git
cd NovaLang
dotnet publish -c Release

# Run examples  
novalang examples/complete_guide.sf    # Complete tutorial
novalang examples/interactive_demo.sf  # Interactive program
novalang repl                          # Interactive shell
```

## 📊 Technical Details

- **Platform**: .NET 9.0, C# 12
- **Architecture**: Recursive descent parser + tree-walking interpreter  
- **Distribution**: Single-file executable, cross-platform
- **Language Features**: 45+ tokens, 25+ AST nodes, complete M3 features
- **Examples**: 5 essential files (complete_guide.sf, readme_demo.sf, interactive_demo.sf, input_test.sf, print_test.sf)

## 🔮 Roadmap

**v1.1.0** (Planned):
- Rest parameters `function(...args)`
- Enhanced standard library  
- Module system (import/export)

**v2.0.0** (Future):
- JIT compilation for performance
- Async/await support
- Developer tools (debugger, profiler)

## 🏆 Achievement

NovaLang v1.0.0-alpha represents a **complete functional programming language** with:
- **Production Quality**: All features thoroughly tested and validated
- **Real-World Ready**: Standalone executable, comprehensive examples, full documentation
- **JavaScript Familiarity**: Familiar syntax with functional programming benefits
- **Developer Friendly**: REPL, clear errors, extensive examples

**Ready to explore functional programming with familiar syntax?**

Download, build, and run `novalang examples/complete_guide.sf` to get started! 🚀

---
*MIT License | Built with .NET 9.0 | Cross-Platform Ready*
