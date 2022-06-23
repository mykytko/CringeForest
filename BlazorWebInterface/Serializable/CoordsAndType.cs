using System.Text.Json.Serialization;

namespace BlazorWebInterface.Serializable;

public class CoordsAndType
{
    public int X { get; set; }
    public int Y { get; set; }
    public int AnimalType { get; set; }
        
    [JsonConstructor]
    public CoordsAndType(int x, int y, int animalType)
    {
        X = x;
        Y = y;
        AnimalType = animalType;
    }
}