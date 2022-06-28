using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CringeForestLibrary;

public class SerializableMap
{
    public int[][] Matrix { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public Dictionary<int, FoodSupplier> FoodSuppliers { get; set; } // encoded coordinate is i * height + j
    public Dictionary<int, Animal> Animals { get; set; }
    
    [JsonConstructor]
    public SerializableMap(int[][] matrix, int height, int width,
        Dictionary<int, FoodSupplier> foodSuppliers, Dictionary<int, Animal> animals)
    {
        Matrix = matrix;
        Height = height;
        Width = width;
        FoodSuppliers = foodSuppliers;
        Animals = animals;
    }

    /*
     * Makes copy of given map and stores it into inner fields
     * (sizes, tables of pixels, foods(encoded), animals)
     */
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
                Matrix[i][j] = map.Matrix[i, j].BiomeId;
            }
        }

        FoodSuppliers = new Dictionary<int, FoodSupplier>();
        foreach (var foodSupplier in map.Food)
        {
            FoodSuppliers.Add(foodSupplier.Key.Item1 * Height + foodSupplier.Key.Item2, foodSupplier.Value);
        }

        Animals = new Dictionary<int, Animal>();
        foreach (var animal in map.AnimalsById.Values)
        {
            Animals.Add(animal.Id, animal);
        }
    }
}