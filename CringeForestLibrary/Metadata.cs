using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace CringeForestLibrary
{
    public class BiomeType
    {
    }
    
    public class FoodType
    {
        public Dictionary<int, float> Frequency { get; }
        public int Saturation { get; }
        public int GrowthRate { get; }
    }

    public class Food
    {
        private int _type;

        public int Saturation
        {
            get => Saturation;
            set
            {
                if (value < 0 || value > Metadata.FoodTypes[_type].Saturation)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public Food(int type)
        {
            _type = type;
            Saturation = Metadata.FoodTypes[_type].Saturation;
        }
    }

    public class AnimalType
    {
        public Dictionary<int, float> Frequency { get; }
        public Dictionary<int, FoodType> FoodTypes { get; }
        public bool IsPredatory { get; }
        private int _foodIntake;
        private int _speed;
        private int _maxAge;
    }

    public static class Metadata
    {
        public static Dictionary<int, BiomeType> BiomeTypes;
        public static Dictionary<int, FoodType> FoodTypes;
        public static Dictionary<int, AnimalType> AnimalTypes;

        static Metadata()
        {
            Trace.WriteLine("Constructing Metadata...");
            FoodTypes = new Dictionary<int, FoodType>();
            AnimalTypes = new Dictionary<int, AnimalType>();
        }

        public static bool InitializeMetadata(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Trace.WriteLine("Specified configuration " + fileName + " does not exist!");
                return false;
            }

            Trace.WriteLine("Reading the configuration...");
            string jsonMetadata = File.ReadAllText(fileName); 
            var jsonDocument = JsonDocument.Parse(jsonMetadata);
            JsonProperty biomeTypes = default;
            JsonProperty foodTypes = default;
            JsonProperty animalTypes = default;
            bool biomeTypesInit = false;
            bool foodTypesInit = false;
            bool animalTypesInit = false;
            Trace.WriteLine("Parsing the configuration...");
            foreach (var element in jsonDocument.RootElement.EnumerateObject())
            {
                if (element.NameEquals("Biomes"))
                {
                    if (biomeTypesInit)
                    {
                        Trace.WriteLine("Two conflicting Biomes entries in " + fileName);
                        return false;
                    }

                    biomeTypesInit = true;
                    biomeTypes = element;
                    continue;
                }
                
                if (element.NameEquals("FoodTypes"))
                {
                    if (foodTypesInit)
                    {
                        Trace.WriteLine("Two conflicting FoodTypes entries in " + fileName);
                        return false;
                    }

                    foodTypesInit = true;
                    foodTypes = element;
                    continue;
                }

                if (element.NameEquals("AnimalTypes"))
                {
                    if (animalTypesInit)
                    {
                        Trace.WriteLine("Two conflicting AnimalTypes entries in " + fileName);
                        return false;
                    }

                    animalTypesInit = true;
                    animalTypes = element;
                    continue;
                }

                Trace.WriteLine("Invalid key in " + fileName + ": " + element.Name);
            }
            
            BiomeTypes = new Dictionary<int, BiomeType>();
            int i = 1;
            foreach (var element in biomeTypes.Value.EnumerateObject())
            {
                BiomeTypes.Add(i, JsonSerializer.Deserialize<BiomeType>(element.Value.GetRawText()));
                i++;
            }

            FoodTypes = new Dictionary<int, FoodType>();
            i = 1;
            foreach (var element in foodTypes.Value.EnumerateObject())
            {
                FoodTypes.Add(i, JsonSerializer.Deserialize<FoodType>(element.Value.GetRawText()));
                i++;
            }

            AnimalTypes = new Dictionary<int, AnimalType>();
            i = 1;
            foreach (var element in animalTypes.Value.EnumerateObject())
            {
                AnimalTypes.Add(i, JsonSerializer.Deserialize<AnimalType>(element.Value.GetRawText()));
                i++;
            }

            Trace.WriteLine("Metadata initialized");
            return true;
        }
    }
}