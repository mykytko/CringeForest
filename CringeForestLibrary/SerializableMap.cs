using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CringeForestLibrary;

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

public class SerializableMap
{
    public int[][] Matrix { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public Dictionary<int, FoodSupplier> FoodSuppliers { get; set; } // encoded coordinate is i * height + j
    public Dictionary<int, CoordsAndType> Animals { get; set; }


    [JsonConstructor]
    public SerializableMap(int[][] matrix, int height, int width,
        Dictionary<int, FoodSupplier> foodSuppliers, Dictionary<int, CoordsAndType> animals)
    {
        Matrix = matrix;
        Height = height;
        Width = width;
        FoodSuppliers = foodSuppliers;
        Animals = animals;
    }

    public SerializableMap(Map map)
    {
        Height = map.Height;
        Width = map.Width;
        Matrix = new int[Height][];
        for (var i = 0; i < Height; i++)
        {
            Matrix[i] = new int[Width];
            for (var j = 0; j < Width; j++)
            {
                Matrix[i][j] = map.Matrix[j, i].BiomeId;
            }
        }

        FoodSuppliers = new Dictionary<int, FoodSupplier>();
        foreach (var foodSupplier in map.Food)
        {
            FoodSuppliers.Add(foodSupplier.Key.Item2 * Width + foodSupplier.Key.Item1, foodSupplier.Value);
        }

        Animals = new Dictionary<int, CoordsAndType>();
        foreach (var animal in map.AnimalsById.Values)
        {
            Animals.Add(animal.Id, new CoordsAndType(animal.X, animal.Y, animal.Type));
        }
    }
}