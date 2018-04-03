# Refer

This library aims to manage references to fields or properties nested in other structures, similar in its idea to the `ref` keyword. Unlike with the `ref` keyword the wrapped property or field can be dynamically reassigned to a different base object, so the binding is not to a place in memory, but to a path pointing inside a data structure, which is itself similar to the concept of lenses.

## Example

```c#
using Refer;
using static System.Console;

public class Bar
{
    public int X { get; set; }
}
public class Foo
{
    public Bar B { get; } = new Bar();
}

public class Program
{
    public static void Main()
    {
        var foo = new Foo()
        {
            B = {
                X = 5,
            },
        };
        
        var reference = foo.Bind(f => f.B.X);
        WriteLine(reference); // 5
        
        float value = reference; // Implicitly converts to its underlying type,
                                 // in this case int, which initializes the float value
        WriteLine(value == 5); // True
        
        reference.Value = 7;
        WriteLine(foo.B.X); // 7
    }
}
```
