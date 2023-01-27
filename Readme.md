# InterpolatedStringFormatter
Interpolates strings that are formatted with named variables


## Example
```c#
var mystring = "a thing(and something {other})";
Console.WriteLine(mystring.Interpolate("else"));
```

Outputs:
```
a thing(and something else)
```

## Why ?

Because String Interpolation is done by the compiler, not at runtime.
```c#
string.Format(mystring, "else");
```
Gives you:

```
Input string was not in a correct format.
```
Because the format is looking for `{0}` and not a named variable.

## Compatibility

.NetStandard 2.0