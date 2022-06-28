using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic.CompilerServices;

namespace CringeForestLibrary
{
    public class BiomeSpecification
    {
        public string Name { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public Dictionary<int, int> AnimalShares { get; set; }
        public Dictionary<int, int> FoodShares { get; set; }
    }
    
    public class FoodSpecification
    {
        public string Name { get; set; }
        public int Saturation { get; set; }
        public double GrowthRate { get; set; }
    }
    
    public class AnimalSpecification
    {
        public string Name { get; set; }
        public HashSet<int> FoodTypes { get; set; }
        public bool IsPredatory { get; set; }
        public int FoodIntake { get; set; }
        public int Speed { get; set; }
        public int MaxAge { get; set; }
    }

    public class FoodSupplier
    {
        private int _saturation;
        public int Saturation
        {
            get => _saturation;
            set
            {
                if (value < 0 || value > Metadata.FoodSpecifications[FoodType].Saturation)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                
                _saturation = value;
            }
        }
        public int FoodType { get; set; }

        [JsonConstructor]
        public FoodSupplier(int foodType, int saturation)
        {
            FoodType = foodType;
            Saturation = saturation;
        }
        
        public FoodSupplier(int foodType)
        {
            FoodType = foodType;
            Saturation = Metadata.FoodSpecifications[FoodType].Saturation;
        }
    }

    public static class Metadata
    {
        public static List<BiomeSpecification> BiomeSpecifications { get; set; }
        public static List<FoodSpecification> FoodSpecifications { get; set; }
        public static List<AnimalSpecification> AnimalSpecifications { get; set; }
        
        public static bool InitializeMetadata(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Trace.WriteLine("Specified configuration " + fileName + " does not exist!");
                return false;
            }

            Trace.WriteLine("Reading the configuration...");
            var jsonMetadata = File.ReadAllText(fileName); 
            var jsonDocument = JsonDocument.Parse(jsonMetadata);

            string foodSpecsJson = null;
            string animalSpecsJson = null;
            string biomeSpecsJson = null;

            Trace.WriteLine("Parsing the configuration...");
            foreach (var element in jsonDocument.RootElement.EnumerateObject())
            {
                switch (element.Name)
                {
                    case "FoodSpecifications":
                        if (foodSpecsJson != null)
                        {
                            Trace.WriteLine("Two conflicting FoodTypes entries in " + fileName);
                            return false;
                        }
                        
                        foodSpecsJson = element.Value.GetRawText();
                        break;
                    
                    case "AnimalSpecifications":
                        if (animalSpecsJson != null)
                        {
                            Trace.WriteLine("Two conflicting FoodTypes entries in " + fileName);
                            return false;
                        }

                        animalSpecsJson = element.Value.GetRawText();
                        break;
                    
                    case "BiomeSpecifications":
                        if (biomeSpecsJson != null)
                        {
                            Trace.WriteLine("Two conflicting AnimalTypes entries in " + fileName);
                            return false;
                        }

                        biomeSpecsJson = element.Value.GetRawText();
                        break;
                    
                    default:
                        Trace.WriteLine("Invalid key in " + fileName + ": " + element.Name);
                        return false;
                }
            }

            Debug.Assert(foodSpecsJson != null, nameof(foodSpecsJson) + " != null");
            Debug.Assert(animalSpecsJson != null, nameof(animalSpecsJson) + " != null");
            Debug.Assert(biomeSpecsJson != null, nameof(biomeSpecsJson) + " != null");
            
            FoodSpecifications = JsonSerializer.Deserialize<List<FoodSpecification>>(foodSpecsJson);

            var tempFoodDictionary = new Dictionary<string, int>();
            Debug.Assert(FoodSpecifications != null, nameof(FoodSpecifications) + " != null");
            for (var i = 0; i < FoodSpecifications.Count; i++)
            {
                tempFoodDictionary.Add(FoodSpecifications[i].Name, i);
            }

            ReplaceNamesById(ref animalSpecsJson, "FoodTypes",
                match => tempFoodDictionary[match.Value[1..^1]].ToString());
            AnimalSpecifications = JsonSerializer.Deserialize<List<AnimalSpecification>>(animalSpecsJson);
            
            var tempAnimalDictionary = new Dictionary<string, int>();
            Debug.Assert(AnimalSpecifications != null, nameof(AnimalSpecifications) + " != null");
            for (var i = 0; i < AnimalSpecifications.Count; i++)
            {
                Debug.Assert(AnimalSpecifications != null, nameof(AnimalSpecifications) + " != null");
                tempAnimalDictionary.Add(AnimalSpecifications[i].Name, i);
            }

            ReplaceNamesById(ref biomeSpecsJson, "FoodShares", 
                match => '"' + tempFoodDictionary[match.Value[1..^1]].ToString() + '"');
            ReplaceNamesById(ref biomeSpecsJson, "AnimalShares", 
                match => '"' + tempAnimalDictionary[match.Value[1..^1]].ToString() + '"');
            BiomeSpecifications = JsonSerializer.Deserialize<List<BiomeSpecification>>(biomeSpecsJson);

            Trace.WriteLine("Metadata initialized");
            return true;
        }

        private static void ReplaceNamesById(ref string specsJson, string name, MatchEvaluator evaluator)
        {
            var builder = new StringBuilder();
            var index = 0;
            while (true)
            {
                var begin = specsJson.IndexOf(name, index, StringComparison.Ordinal);
                if (begin == -1)
                {
                    builder.Append(specsJson[index..]);
                    break;
                }
                var square = specsJson.IndexOf('[', begin);
                var curly = specsJson.IndexOf('{', begin);
                char bracket;
                if (square == -1)
                {
                    bracket = '}';
                    begin = curly;
                }
                else if (curly == -1)
                {
                    bracket = ']';
                    begin = square;
                }
                else
                {
                    bracket = square < curly ? ']' : '}';
                    begin = square < curly ? square : curly;
                }
                builder.Append(specsJson.Substring(index, begin - index));
                
                var end = specsJson.IndexOf(bracket, begin);
                const string pattern = "\"[A-Za-z]*\"";
                builder.Append(
                    Regex.Replace(specsJson.Substring(begin, end - begin + 1), pattern, evaluator));
                index = end + 1;
            }
            
            specsJson = builder.ToString();
        }
    }
}