using Xunit;
using NovaLang.Runtime;

namespace NovaLang.Tests.Unit
{
    public class EnvironmentTests
    {
        [Fact]
        public void TestDefineAndGet()
        {
            var env = new Environment();
            var value = new NumberValue(42);
            
            env.Define("x", value);
            var retrieved = env.Get("x");
            
            Assert.Equal(value, retrieved);
        }

        [Fact]
        public void TestAssign()
        {
            var env = new Environment();
            var initialValue = new NumberValue(42);
            var newValue = new NumberValue(100);
            
            env.Define("x", initialValue);
            env.Assign("x", newValue);
            var retrieved = env.Get("x");
            
            Assert.Equal(newValue, retrieved);
        }

        [Fact]
        public void TestUndefinedVariable()
        {
            var env = new Environment();
            
            Assert.Throws<RuntimeException>(() => env.Get("undefined"));
        }

        [Fact]
        public void TestNestedEnvironment()
        {
            var global = new Environment();
            var local = new Environment(global);
            
            global.Define("x", new NumberValue(42));
            local.Define("y", new NumberValue(100));
            
            // Local can access both local and global
            Assert.Equal(42.0, ((NumberValue)local.Get("x")).Value);
            Assert.Equal(100.0, ((NumberValue)local.Get("y")).Value);
            
            // Global can only access global
            Assert.Equal(42.0, ((NumberValue)global.Get("x")).Value);
            Assert.Throws<RuntimeException>(() => global.Get("y"));
        }

        [Fact]
        public void TestShadowing()
        {
            var global = new Environment();
            var local = new Environment(global);
            
            global.Define("x", new NumberValue(42));
            local.Define("x", new NumberValue(100));
            
            // Local shadows global
            Assert.Equal(100.0, ((NumberValue)local.Get("x")).Value);
            Assert.Equal(42.0, ((NumberValue)global.Get("x")).Value);
        }

        [Fact]
        public void TestGlobalEnvironment()
        {
            var global = Environment.CreateGlobal();
            
            // Should have console object
            var console = global.Get("console");
            Assert.IsType<ObjectValue>(console);
            
            // Should have Math object
            var math = global.Get("Math");
            Assert.IsType<ObjectValue>(math);
            
            // Should have Array object
            var array = global.Get("Array");
            Assert.IsType<ObjectValue>(array);
        }

        [Fact]
        public void TestBuiltinFunctions()
        {
            var global = Environment.CreateGlobal();
            
            // Test typeof function
            var typeofFunc = global.Get("typeof");
            Assert.IsType<NativeFunctionValue>(typeofFunc);
            
            var result = ((NativeFunctionValue)typeofFunc).Call(new[] { new NumberValue(42) }, global);
            Assert.Equal("number", ((StringValue)result).Value);
        }
    }
}
