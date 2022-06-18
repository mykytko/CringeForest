using System.Text.Json.Serialization;

namespace WebInterface.Serializable;

public class CoordsAndRatio {
    public int X { get; set; }
    public int Y { get; set; }
    public double Ratio { get; set; }
            
    public CoordsAndRatio((int, int) coords, double ratio)
    {
        X = coords.Item1;
        Y = coords.Item2;
        Ratio = ratio;
    }

    [JsonConstructor]
    public CoordsAndRatio(int x, int y, double ratio)
    {
        X = x;
        Y = y;
        Ratio = ratio;
    }
}