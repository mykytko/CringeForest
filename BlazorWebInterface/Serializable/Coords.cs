using System.Text.Json.Serialization;

namespace WebInterface.Serializable;

public class Coords
{
    public int X { get; set; }
    public int Y { get; set; }

    [JsonConstructor]
    public Coords(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Coords((int, int) coords)
    {
        (X, Y) = coords;
    }
}