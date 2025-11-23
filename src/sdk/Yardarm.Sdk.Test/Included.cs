namespace Yardarm.Sdk.Test;

public class Included
{
    // Ensure that a file may be included and reference built-in files

    public static string SerializeDecimal(decimal value)
    {
        return new Serialization.Literals.Converters.DecimalLiteralConverter().Write(value, "decimal");
    }
}
