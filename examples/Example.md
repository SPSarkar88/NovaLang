# NovaLang Complete Tutorial and Examples

Welcome to the complete NovaLang tutorial! This guide takes you step-by-step from basic concepts to advanced features, perfect for beginner developers.

## 📚 Table of Contents

1. [Getting Started](#1-getting-started)
2. [Basic Syntax](#2-basic-syntax)
3. [Working with Data](#3-working-with-data)
4. [Functions](#4-functions)
5. [Input and Output](#5-input-and-output)
6. [Control Flow](#6-control-flow)
7. [Arrays and Objects](#7-arrays-and-objects)
8. [Advanced Features (M3)](#8-advanced-features-m3)
9. [Real-World Examples](#9-real-world-examples)
10. [Best Practices](#10-best-practices)

---

## 1. Getting Started

### What is NovaLang?

NovaLang is a functional programming language with JavaScript-like syntax. It's designed to be:
- **Easy to learn** - Familiar syntax if you know JavaScript
- **Functional** - Focuses on functions and immutable data
- **Safe** - No classes or complex inheritance

### Running Examples

Save any code block as a `.sf` file and run it:
```bash
# Save code as example.sf, then run:
novalang example.sf
```

---

## 2. Basic Syntax

### 2.1 Your First Program

Let's start with the classic "Hello World":

```javascript
// This is a comment
print("Hello, NovaLang!");
```

**What's happening here:**
- `//` creates a comment (ignored by the program)
- `print()` displays text to the console
- Strings are wrapped in double quotes `""`

**Try it yourself:** Save this as `hello.sf` and run `novalang hello.sf`

### 2.2 Variables

Variables store data that can change:

```javascript
// Declaring variables with 'let'
let name = "Alice";
let age = 25;
let isStudent = true;

// Display the variables
print("Name:", name);
print("Age:", age);
print("Is student:", isStudent);

// Variables can be reassigned
age = 26;
print("Updated age:", age);
```

**Key concepts:**
- `let` creates a variable that can change
- Variables can store text (strings), numbers, or true/false (booleans)
- Use `=` to assign values

### 2.3 Constants

Constants are variables that never change:

```javascript
// Constants with 'const' - cannot be reassigned
const PI = 3.14159;
const COMPANY_NAME = "Tech Corp";

print("Pi is approximately:", PI);
print("Company:", COMPANY_NAME);

// This would cause an error:
// PI = 3.14; // Cannot reassign a constant!
```

**When to use const vs let:**
- Use `const` for values that never change (like math constants, configuration)
- Use `let` for values that might change (like counters, user input)

---

## 3. Working with Data

### 3.1 Data Types

NovaLang has several types of data:

```javascript
// Numbers (integers and decimals)
let wholeNumber = 42;
let decimal = 3.14;
let negative = -10;

// Strings (text)
let singleWord = "Hello";
let sentence = "This is a complete sentence.";
let empty = "";

// Booleans (true or false)
let isReady = true;
let isFinished = false;

// Special values
let nothing = null;
let notDefined = undefined;

// Display all types
print("Number:", wholeNumber);
print("Decimal:", decimal);
print("String:", sentence);
print("Boolean:", isReady);
print("Null:", nothing);
print("Undefined:", notDefined);
```

### 3.2 Basic Math

NovaLang supports standard mathematical operations:

```javascript
let a = 10;
let b = 3;

// Basic operations
print("Addition:", a + b);      // 13
print("Subtraction:", a - b);   // 7
print("Multiplication:", a * b); // 30
print("Division:", a / b);      // 3.333...
print("Remainder:", a % b);     // 1

// Order of operations (PEMDAS)
let result = 2 + 3 * 4;  // 14, not 20
print("2 + 3 * 4 =", result);

let withParentheses = (2 + 3) * 4;  // 20
print("(2 + 3) * 4 =", withParentheses);
```

### 3.3 Working with Strings

Strings are sequences of characters:

```javascript
let firstName = "John";
let lastName = "Doe";

// Combining strings (concatenation)
let fullName = firstName + " " + lastName;
print("Full name:", fullName);

// String length and properties
print("First name has", firstName.length, "characters");

// Template literals - easier way to combine strings and variables
let age = 30;
let introduction = `Hello, my name is ${firstName} and I'm ${age} years old.`;
print(introduction);

// Multiple variables in template literals
let city = "New York";
let country = "USA";
let location = `I live in ${city}, ${country}.`;
print(location);
```

**Template literals explained:**
- Use backticks `` ` `` instead of quotes
- Variables go inside `${variable_name}`
- Much easier than using `+` to join strings

---

## 4. Functions

Functions are reusable blocks of code that perform specific tasks.

### 4.1 Basic Functions

```javascript
// Function declaration
function greet() {
    print("Hello from a function!");
}

// Calling (using) the function
greet();

// Function with parameters
function greetPerson(name) {
    print("Hello,", name + "!");
}

// Calling with different names
greetPerson("Alice");
greetPerson("Bob");
greetPerson("Charlie");
```

**Key concepts:**
- `function` keyword declares a function
- Parameters (like `name`) are inputs to the function
- Call a function by writing its name with parentheses: `greet()`

### 4.2 Functions that Return Values

```javascript
// Function that calculates and returns a result
function add(a, b) {
    return a + b;
}

// Using the returned value
let sum = add(5, 3);
print("5 + 3 =", sum);

// Functions can return different types
function getGreeting(name) {
    return "Hello, " + name + "!";
}

let message = getGreeting("Alice");
print(message);

// Function with multiple operations
function calculateTip(billAmount, tipPercent) {
    let tip = billAmount * (tipPercent / 100);
    let total = billAmount + tip;
    return total;
}

let bill = 50.00;
let tipRate = 18;
let finalAmount = calculateTip(bill, tipRate);
print(`Bill: $${bill}, Tip: ${tipRate}%, Total: $${finalAmount}`);
```

### 4.3 Arrow Functions (Shorthand)

Arrow functions are a shorter way to write functions:

```javascript
// Traditional function
function multiply(a, b) {
    return a * b;
}

// Arrow function - same thing, shorter syntax
const multiplyArrow = (a, b) => {
    return a * b;
};

// Even shorter for simple functions
const square = x => x * x;
const double = x => x * 2;

// Using arrow functions
print("multiply(4, 5):", multiply(4, 5));
print("multiplyArrow(4, 5):", multiplyArrow(4, 5));
print("square(7):", square(7));
print("double(8):", double(8));
```

**When to use arrow functions:**
- For short, simple functions
- When you want concise syntax
- Traditional functions are fine too - use what feels comfortable!

---

## 5. Input and Output

### 5.1 Output Functions

NovaLang provides two ways to display output:

```javascript
// Method 1: print() - simple and convenient
print("This is the print function");
print("You can print", "multiple", "values", 123, true);

// Method 2: console.log() - identical to print()
console.log("This is console.log");
console.log("It works", "exactly the same", "as print");

// Both are interchangeable - use whichever you prefer
print("I prefer print()");
console.log("I prefer console.log()");
```

### 5.2 Getting User Input

```javascript
// Basic input - no prompt
print("Please enter your name:");
let userName = input();
print("Hello,", userName + "!");

// Input with prompt - more user-friendly
let favoriteColor = input("What's your favorite color? ");
print("Nice choice!", favoriteColor, "is a great color.");

// Building an interactive program
let name = input("Enter your name: ");
let age = input("Enter your age: ");
let city = input("Where do you live? ");

print("\n--- User Profile ---");
print(`Name: ${name}`);
print(`Age: ${age}`);
print(`City: ${city}`);
print(`${name} is ${age} years old and lives in ${city}.`);
```

### 5.3 Interactive Calculator

Let's build a simple calculator using input and output:

```javascript
print("🧮 Simple Calculator");
print("==================");

let num1 = input("Enter first number: ");
let operation = input("Enter operation (+, -, *, /): ");
let num2 = input("Enter second number: ");

print(`\nCalculation: ${num1} ${operation} ${num2}`);
print("Note: NovaLang currently works with strings, so this shows concatenation");
print(`Result as string concatenation: ${num1}${num2}`);

// In a full implementation, you'd convert strings to numbers first
print("\n(In future versions, we'll add number conversion for real math!)");
```

---

## 6. Control Flow

Control flow determines which parts of your program run based on conditions.

### 6.1 If Statements

```javascript
// Basic if statement
let age = 18;

if (age >= 18) {
    print("You are an adult.");
}

// If-else statement
let temperature = 75;

if (temperature > 80) {
    print("It's hot outside!");
} else {
    print("It's not too hot.");
}

// Multiple conditions with else if
let score = 85;

if (score >= 90) {
    print("Grade: A");
} else if (score >= 80) {
    print("Grade: B");
} else if (score >= 70) {
    print("Grade: C");
} else if (score >= 60) {
    print("Grade: D");
} else {
    print("Grade: F");
}
```

### 6.2 Comparison Operators

```javascript
let a = 10;
let b = 5;

// Comparison operators
print("a > b:", a > b);   // true
print("a < b:", a < b);   // false
print("a >= b:", a >= b); // true
print("a <= b:", a <= b); // false
print("a == b:", a == b); // false (equal)
print("a != b:", a != b); // true (not equal)

// Equality vs strict equality
let num = 5;
let str = "5";
print("5 == '5':", num == str);   // Compares value
print("5 === '5':", num === str); // Compares value AND type
```

### 6.3 Logical Operators

```javascript
let age = 25;
let hasLicense = true;
let hasInsurance = true;

// AND operator (&&) - both conditions must be true
if (age >= 18 && hasLicense) {
    print("You can drive!");
}

// OR operator (||) - at least one condition must be true
if (hasLicense || age >= 21) {
    print("You meet at least one requirement.");
}

// NOT operator (!) - flips true/false
if (!hasInsurance) {
    print("You need insurance to drive.");
} else {
    print("Good, you have insurance!");
}

// Combining logical operators
if (age >= 18 && hasLicense && hasInsurance) {
    print("You're fully qualified to drive!");
} else {
    print("You're missing some requirements.");
}
```

### 6.4 Loops

Loops repeat code multiple times:

```javascript
// For loop - when you know how many times to repeat
print("Counting from 1 to 5:");
for (let i = 1; i <= 5; i = i + 1) {
    print("Count:", i);
}

// While loop - repeat while condition is true
print("\nCountdown:");
let countdown = 5;
while (countdown > 0) {
    print(countdown);
    countdown = countdown - 1;
}
print("Blast off! 🚀");

// For loop with arrays (we'll learn arrays next)
print("\nLoop through items:");
let fruits = ["apple", "banana", "orange"];
for (let i = 0; i < fruits.length; i = i + 1) {
    print("Fruit", i + 1 + ":", fruits[i]);
}
```

---

## 7. Arrays and Objects

### 7.1 Arrays (Lists of Items)

Arrays store multiple items in order:

```javascript
// Creating arrays
let numbers = [1, 2, 3, 4, 5];
let colors = ["red", "green", "blue"];
let mixed = [1, "hello", true, null];

// Accessing array items (0-based indexing)
print("First number:", numbers[0]);  // 1
print("Second color:", colors[1]);   // "green"
print("Last number:", numbers[4]);   // 5

// Array properties
print("Numbers array length:", numbers.length);
print("Colors array length:", colors.length);

// Displaying entire arrays
print("All numbers:", numbers);
print("All colors:", colors);

// Working with arrays
let shoppingList = ["milk", "bread", "eggs"];
print("Shopping list:", shoppingList);
print("First item to buy:", shoppingList[0]);
```

### 7.2 Objects (Key-Value Pairs)

Objects store related information together:

```javascript
// Creating objects
let person = {
    name: "Alice",
    age: 30,
    city: "New York",
    isEmployed: true
};

// Accessing object properties
print("Name:", person.name);
print("Age:", person.age);
print("City:", person.city);

// Another way to access properties
print("Employment status:", person["isEmployed"]);

// Objects can contain arrays
let student = {
    name: "Bob",
    grades: [85, 92, 78, 96],
    subjects: ["Math", "Science", "History"]
};

print("Student:", student.name);
print("Grades:", student.grades);
print("First grade:", student.grades[0]);
print("Subjects:", student.subjects);

// Nested objects
let company = {
    name: "Tech Corp",
    address: {
        street: "123 Main St",
        city: "Seattle",
        state: "WA"
    },
    employees: 150
};

print("Company:", company.name);
print("Address:", company.address.street, company.address.city);
```

### 7.3 Arrays of Objects

Combining arrays and objects for complex data:

```javascript
// Array of objects - very common pattern
let users = [
    { name: "Alice", age: 25, role: "developer" },
    { name: "Bob", age: 30, role: "designer" },
    { name: "Charlie", age: 35, role: "manager" }
];

// Accessing array of objects
print("All users:", users);
print("First user:", users[0]);
print("First user's name:", users[0].name);

// Loop through array of objects
print("\nUser directory:");
for (let i = 0; i < users.length; i = i + 1) {
    let user = users[i];
    print(`${user.name} (${user.age}) - ${user.role}`);
}
```

---

## 8. Advanced Features (M3)

These are advanced features that make NovaLang more powerful and expressive.

### 8.1 Template Literals with Expressions

Template literals can contain more than just variables:

```javascript
let name = "Alice";
let age = 25;
let salary = 75000;

// Simple template literals (we've seen these)
print(`Hello, ${name}!`);

// Template literals with expressions
print(`${name} will be ${age + 1} years old next year.`);
print(`Monthly salary: $${salary / 12}`);
print(`In 5 years, ${name} will be ${age + 5}.`);

// Complex expressions
let products = [
    { name: "Laptop", price: 999 },
    { name: "Mouse", price: 25 }
];

print(`The ${products[0].name} costs $${products[0].price}.`);
print(`Total cost: $${products[0].price + products[1].price}`);

// Multi-line template literals
let report = `
User Report
-----------
Name: ${name}
Age: ${age}
Annual Salary: $${salary}
Monthly Salary: $${salary / 12}
`;
print(report);
```

### 8.2 Destructuring - Unpacking Values

Destructuring lets you extract values from arrays and objects easily:

```javascript
// Array destructuring - extract values by position
let colors = ["red", "green", "blue", "yellow"];

// Traditional way
let firstColor = colors[0];
let secondColor = colors[1];

// Destructuring way - much cleaner!
let [first, second, third] = colors;
print("First color:", first);   // "red"
print("Second color:", second); // "green"
print("Third color:", third);   // "blue"

// Skip elements you don't need
let [primary, , secondary] = colors; // Skip "green"
print("Primary:", primary);     // "red"
print("Secondary:", secondary); // "blue"

// Rest elements - capture remaining items
let [main, ...others] = colors;
print("Main color:", main);     // "red"
print("Other colors:", others); // ["green", "blue", "yellow"]

// Object destructuring - extract values by property name
let person = { 
    name: "Alice", 
    age: 30, 
    city: "New York", 
    job: "Developer" 
};

// Traditional way
let personName = person.name;
let personAge = person.age;

// Destructuring way
let { name, age, city } = person;
print("Name:", name);  // "Alice"
print("Age:", age);    // 30
print("City:", city);  // "New York"

// Rename variables while destructuring
let { name: fullName, job: occupation } = person;
print("Full name:", fullName);      // "Alice"
print("Occupation:", occupation);   // "Developer"
```

### 8.3 Spread Syntax - Expanding Values

Spread syntax lets you expand arrays and objects:

```javascript
// Array spread - expand array elements
let fruits = ["apple", "banana"];
let vegetables = ["carrot", "broccoli"];

// Combine arrays using spread
let food = [...fruits, ...vegetables];
print("All food:", food); // ["apple", "banana", "carrot", "broccoli"]

// Add items while spreading
let snacks = ["chips", ...fruits, "cookies"];
print("Snacks:", snacks); // ["chips", "apple", "banana", "cookies"]

// Object spread - expand object properties
let basicInfo = { name: "Alice", age: 30 };
let contactInfo = { email: "alice@example.com", phone: "555-1234" };

// Combine objects using spread
let fullProfile = { ...basicInfo, ...contactInfo };
print("Full profile:", fullProfile);
// { name: "Alice", age: 30, email: "alice@example.com", phone: "555-1234" }

// Override properties while spreading
let updatedProfile = { ...basicInfo, age: 31, city: "Boston" };
print("Updated profile:", updatedProfile);
// { name: "Alice", age: 31, city: "Boston" }

// Practical example: function that accepts multiple arguments
function createUser(name, ...details) {
    print("Creating user:", name);
    print("Additional details:", details);
}

createUser("Bob", "developer", 25, "Seattle");
```

### 8.4 Advanced Array and Object Operations

```javascript
// Working with complex nested data
let company = {
    name: "Tech Innovations",
    departments: [
        {
            name: "Engineering", 
            employees: [
                { name: "Alice", skills: ["JavaScript", "Python"] },
                { name: "Bob", skills: ["Java", "C++"] }
            ]
        },
        {
            name: "Design",
            employees: [
                { name: "Carol", skills: ["Photoshop", "Figma"] }
            ]
        }
    ]
};

// Destructure nested data
let { name: companyName, departments } = company;
let [engineering, design] = departments;
let { employees: engineeringTeam } = engineering;
let [alice, bob] = engineeringTeam;

print("Company:", companyName);
print("First engineer:", alice.name);
print("Alice's skills:", alice.skills);

// Spread with nested data
let newEmployee = { name: "David", skills: ["React", "Node.js"] };
let expandedTeam = [...engineeringTeam, newEmployee];
print("Expanded team:", expandedTeam);

// Complex destructuring with defaults
let config = { 
    theme: "dark", 
    language: "en",
    features: ["notifications", "sync"]
};

let { 
    theme = "light", 
    language = "en", 
    timeout = 5000,  // default value if not present
    features 
} = config;

print("Theme:", theme);      // "dark"
print("Language:", language); // "en"
print("Timeout:", timeout);   // 5000 (default used)
print("Features:", features); // ["notifications", "sync"]
```

---

## 9. Real-World Examples

### 9.1 User Registration System

```javascript
print("🎯 User Registration System");
print("============================");

function registerUser() {
    // Get user information
    let name = input("Enter your full name: ");
    let email = input("Enter your email: ");
    let age = input("Enter your age: ");
    let interests = input("Enter your interests (comma-separated): ");
    
    // Process the data
    let interestsList = interests.split(","); // Note: split() would need implementation
    
    // Create user profile
    let userProfile = {
        name: name,
        email: email,
        age: age,
        interests: interests,
        registeredAt: "2025-08-30", // In real app, would be current date
        isActive: true
    };
    
    // Display confirmation
    print("\n✅ Registration Successful!");
    print("========================");
    print(`Name: ${userProfile.name}`);
    print(`Email: ${userProfile.email}`);
    print(`Age: ${userProfile.age}`);
    print(`Interests: ${userProfile.interests}`);
    print(`Registered: ${userProfile.registeredAt}`);
    
    return userProfile;
}

// Run the registration
let newUser = registerUser();
print("\nUser object created:", newUser);
```

### 9.2 Simple Inventory Manager

```javascript
print("📦 Inventory Management System");
print("==============================");

// Sample inventory
let inventory = [
    { id: 1, name: "Laptop", price: 999.99, quantity: 5 },
    { id: 2, name: "Mouse", price: 24.99, quantity: 20 },
    { id: 3, name: "Keyboard", price: 79.99, quantity: 10 },
    { id: 4, name: "Monitor", price: 299.99, quantity: 3 }
];

function displayInventory(items) {
    print("\n--- Current Inventory ---");
    for (let i = 0; i < items.length; i = i + 1) {
        let item = items[i];
        let totalValue = item.price * item.quantity;
        print(`${item.id}. ${item.name} - $${item.price} (Qty: ${item.quantity}) [Total: $${totalValue}]`);
    }
}

function findExpensiveItems(items, minPrice) {
    print(`\n--- Items over $${minPrice} ---`);
    for (let i = 0; i < items.length; i = i + 1) {
        let item = items[i];
        if (item.price > minPrice) {
            print(`- ${item.name}: $${item.price}`);
        }
    }
}

function calculateTotalValue(items) {
    let total = 0;
    for (let i = 0; i < items.length; i = i + 1) {
        let item = items[i];
        total = total + (item.price * item.quantity);
    }
    return total;
}

// Run inventory operations
displayInventory(inventory);
findExpensiveItems(inventory, 50);

let totalInventoryValue = calculateTotalValue(inventory);
print(`\n💰 Total Inventory Value: $${totalInventoryValue}`);

// Interactive features
let searchPrice = input("\nEnter minimum price to search for expensive items: $");
findExpensiveItems(inventory, parseFloat(searchPrice)); // Note: parseFloat would need implementation
```

### 9.3 Quiz Application

```javascript
print("🧠 NovaLang Knowledge Quiz");
print("==========================");

// Quiz data structure
let quiz = {
    title: "Programming Basics Quiz",
    questions: [
        {
            question: "What keyword is used to declare a variable in NovaLang?",
            options: ["var", "let", "declare", "set"],
            correct: 1 // index of correct answer
        },
        {
            question: "Which symbol is used for template literals?",
            options: ["\"", "'", "`", "~"],
            correct: 2
        },
        {
            question: "What does the spread operator look like?",
            options: ["...", "***", "---", "+++"],
            correct: 0
        }
    ]
};

function runQuiz(quizData) {
    print(`\n📚 ${quizData.title}`);
    print("=".repeat(quizData.title.length + 4));
    
    let score = 0;
    let { questions } = quizData;
    
    for (let i = 0; i < questions.length; i = i + 1) {
        let currentQuestion = questions[i];
        
        print(`\nQuestion ${i + 1}: ${currentQuestion.question}`);
        
        // Display options
        for (let j = 0; j < currentQuestion.options.length; j = j + 1) {
            print(`${j + 1}. ${currentQuestion.options[j]}`);
        }
        
        let userAnswer = input("Enter your choice (1-4): ");
        let answerIndex = parseInt(userAnswer) - 1; // Note: parseInt would need implementation
        
        if (answerIndex === currentQuestion.correct) {
            print("✅ Correct!");
            score = score + 1;
        } else {
            let correctAnswer = currentQuestion.options[currentQuestion.correct];
            print(`❌ Wrong! The correct answer was: ${correctAnswer}`);
        }
    }
    
    // Display final results
    print(`\n🎯 Quiz Complete!`);
    print(`Your score: ${score}/${questions.length}`);
    
    let percentage = (score / questions.length) * 100;
    if (percentage >= 80) {
        print("🌟 Excellent work!");
    } else if (percentage >= 60) {
        print("👍 Good job!");
    } else {
        print("📖 Keep studying!");
    }
    
    return { score: score, total: questions.length, percentage: percentage };
}

// Run the quiz
let results = runQuiz(quiz);
print(`\nFinal Statistics:`);
print(`Correct: ${results.score}`);
print(`Total: ${results.total}`);
print(`Percentage: ${results.percentage}%`);
```

---

## 10. Best Practices

### 10.1 Code Organization

```javascript
// ✅ Good: Clear, descriptive names
let userAge = 25;
let isLoggedIn = true;
let customerOrders = [];

// ❌ Bad: Unclear names
let x = 25;
let flag = true;
let arr = [];

// ✅ Good: Consistent formatting
function calculateTotal(price, tax, discount) {
    let subtotal = price - discount;
    let total = subtotal + (subtotal * tax);
    return total;
}

// ✅ Good: Comments explain why, not what
// Calculate discount based on customer loyalty level
function getDiscount(customerLevel) {
    if (customerLevel === "gold") {
        return 0.15; // 15% discount for gold members
    }
    return 0.05; // 5% standard discount
}
```

### 10.2 Error Prevention

```javascript
// ✅ Good: Check for valid data
function divideNumbers(a, b) {
    if (b === 0) {
        print("Error: Cannot divide by zero!");
        return null;
    }
    return a / b;
}

// ✅ Good: Provide default values
function greetUser(name = "Guest") {
    print(`Hello, ${name}!`);
}

// ✅ Good: Validate input
function processAge(age) {
    if (age < 0 || age > 150) {
        print("Please enter a valid age between 0 and 150.");
        return false;
    }
    return true;
}
```

### 10.3 Modern NovaLang Patterns

```javascript
// ✅ Use destructuring for cleaner code
let user = { name: "Alice", email: "alice@example.com", age: 30 };

// Instead of:
// let name = user.name;
// let email = user.email;

// Do this:
let { name, email } = user;

// ✅ Use spread for combining data
let defaults = { theme: "light", notifications: true };
let userPrefs = { theme: "dark" };
let finalConfig = { ...defaults, ...userPrefs };

// ✅ Use template literals for readable strings
let message = `Welcome ${name}! You have ${unreadCount} unread messages.`;

// ✅ Use arrow functions for short operations
let numbers = [1, 2, 3, 4, 5];
let doubled = numbers.map(x => x * 2); // Note: map would need implementation

// ✅ Group related functionality
let userUtils = {
    validateEmail: (email) => email.includes("@"),
    formatName: (first, last) => `${first} ${last}`,
    calculateAge: (birthYear) => 2025 - birthYear
};
```

---

## 🎉 Congratulations!

You've completed the comprehensive NovaLang tutorial! You now know:

### ✅ **Fundamentals**
- Variables and constants (`let`, `const`)
- Data types (numbers, strings, booleans)
- Basic operations and math

### ✅ **Programming Concepts**
- Functions (regular and arrow functions)
- Control flow (if/else, loops)
- Input and output (`print`, `input`)

### ✅ **Data Structures**
- Arrays for lists of items
- Objects for structured data
- Combining arrays and objects

### ✅ **Advanced Features**
- Template literals with `${variable}` syntax
- Destructuring for unpacking values
- Spread syntax for combining data

### ✅ **Real-World Skills**
- Building interactive programs
- Organizing code effectively
- Following best practices

## 🚀 Next Steps

1. **Practice**: Try modifying the examples in this guide
2. **Experiment**: Create your own small programs
3. **Build**: Start with simple projects like calculators or to-do lists
4. **Learn More**: Explore advanced programming concepts

## 📁 Quick Reference

**Save and run any example:**
```bash
# Save code as filename.sf
novalang filename.sf
```

**Basic syntax reminders:**
- Variables: `let name = "value"`
- Constants: `const PI = 3.14`
- Functions: `function name() { ... }`
- Arrays: `[1, 2, 3]`
- Objects: `{ key: "value" }`
- Template literals: `` `Hello ${name}!` ``

Happy coding with NovaLang! 🌟
