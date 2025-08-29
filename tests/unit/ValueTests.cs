using Xunit;
using NovaLang.Runtime;
using System.Collections.Generic;

namespace NovaLang.Tests.Unit
{
    public class ValueTests
    {
        [Fact]
        public void TestNumberValue()
        {
            var num = new NumberValue(42.5);
            Assert.Equal(42.5, num.Value);
            Assert.Equal(NovaValueType.Number, num.Type);
            Assert.True(num.IsTruthy);
            Assert.Equal("42.5", num.ToString());
        }

        [Fact]
        public void TestStringValue()
        {
            var str = new StringValue("hello");
            Assert.Equal("hello", str.Value);
            Assert.Equal(NovaValueType.String, str.Type);
            Assert.True(str.IsTruthy);
            Assert.Equal("hello", str.ToString());
        }

        [Fact]
        public void TestBooleanValue()
        {
            var trueVal = BooleanValue.True;
            var falseVal = BooleanValue.False;
            
            Assert.True(trueVal.Value);
            Assert.False(falseVal.Value);
            Assert.True(trueVal.IsTruthy);
            Assert.False(falseVal.IsTruthy);
            Assert.Equal("true", trueVal.ToString());
            Assert.Equal("false", falseVal.ToString());
        }

        [Fact]
        public void TestNullValue()
        {
            var nullVal = NullValue.Instance;
            Assert.Equal(NovaValueType.Null, nullVal.Type);
            Assert.False(nullVal.IsTruthy);
            Assert.Equal("null", nullVal.ToString());
        }

        [Fact]
        public void TestUndefinedValue()
        {
            var undefinedVal = UndefinedValue.Instance;
            Assert.Equal(NovaValueType.Undefined, undefinedVal.Type);
            Assert.False(undefinedVal.IsTruthy);
            Assert.Equal("undefined", undefinedVal.ToString());
        }

        [Fact]
        public void TestArrayValue()
        {
            var elements = new List<NovaValue> { new NumberValue(1), new NumberValue(2), new NumberValue(3) };
            var arr = new ArrayValue(elements);
            
            Assert.Equal(NovaValueType.Array, arr.Type);
            Assert.True(arr.IsTruthy);
            Assert.Equal(3, arr.Elements.Count);
            Assert.Equal("[1, 2, 3]", arr.ToString());
        }

        [Fact]
        public void TestObjectValue()
        {
            var properties = new Dictionary<string, NovaValue>
            {
                ["a"] = new NumberValue(1),
                ["b"] = new StringValue("test")
            };
            var obj = new ObjectValue(properties);
            
            Assert.Equal(NovaValueType.Object, obj.Type);
            Assert.True(obj.IsTruthy);
            Assert.Equal(2, obj.Properties.Count);
            Assert.Contains("a", obj.Properties.Keys);
            Assert.Contains("b", obj.Properties.Keys);
        }

        [Fact]
        public void TestValueOperationsAdd()
        {
            var result1 = ValueOperations.Add(new NumberValue(5), new NumberValue(3));
            Assert.Equal(8.0, ((NumberValue)result1).Value);
            
            var result2 = ValueOperations.Add(new StringValue("hello"), new StringValue(" world"));
            Assert.Equal("hello world", ((StringValue)result2).Value);
            
            var result3 = ValueOperations.Add(new NumberValue(5), new StringValue("3"));
            Assert.Equal("53", ((StringValue)result3).Value);
        }

        [Fact]
        public void TestValueOperationsSubtract()
        {
            var result = ValueOperations.Subtract(new NumberValue(10), new NumberValue(3));
            Assert.Equal(7.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestValueOperationsMultiply()
        {
            var result = ValueOperations.Multiply(new NumberValue(4), new NumberValue(3));
            Assert.Equal(12.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestValueOperationsDivide()
        {
            var result = ValueOperations.Divide(new NumberValue(15), new NumberValue(3));
            Assert.Equal(5.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestValueOperationsModulo()
        {
            var result = ValueOperations.Modulo(new NumberValue(10), new NumberValue(3));
            Assert.Equal(1.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestValueOperationsExponentiate()
        {
            var result = ValueOperations.Exponentiate(new NumberValue(2), new NumberValue(3));
            Assert.Equal(8.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestValueOperationsEqual()
        {
            Assert.True(ValueOperations.IsEqual(new NumberValue(5), new NumberValue(5)));
            Assert.False(ValueOperations.IsEqual(new NumberValue(5), new NumberValue(3)));
            Assert.True(ValueOperations.IsEqual(new StringValue("hello"), new StringValue("hello")));
            Assert.False(ValueOperations.IsEqual(new NumberValue(5), new StringValue("5")));
        }

        [Fact]
        public void TestValueOperationsIsTruthy()
        {
            Assert.True(ValueOperations.IsTruthy(new NumberValue(5)));
            Assert.False(ValueOperations.IsTruthy(new NumberValue(0)));
            Assert.True(ValueOperations.IsTruthy(new StringValue("hello")));
            Assert.False(ValueOperations.IsTruthy(new StringValue("")));
            Assert.True(ValueOperations.IsTruthy(BooleanValue.True));
            Assert.False(ValueOperations.IsTruthy(BooleanValue.False));
            Assert.False(ValueOperations.IsTruthy(NullValue.Instance));
            Assert.False(ValueOperations.IsTruthy(UndefinedValue.Instance));
        }
    }
}
