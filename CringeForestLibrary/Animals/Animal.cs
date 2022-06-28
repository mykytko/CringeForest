using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Transactions;

namespace CringeForestLibrary
{
    public enum AnimalSex
    {
        Male,
        Female
    }

    internal abstract partial class Brain
    {
    }
    
    internal partial class PredatoryBrain : Brain
    {
    }

    internal partial class NotPredatoryBrain : Brain
    {
    }

    public class Animal
    {
        //BTW: these parameter should be adjusted manually
        //so that the simulation runs as long and dynamically as possible
        //DefaultFieldOfView should be moved to specification.json
        //and have different value for different animals
        private const int DefaultFieldOfView = 8;

        private static int _currentId = 0;
        public int Id { get; set; }
        public int AnimalType { get; set; }
        public AnimalSex Sex { get; set; }
        public int Saturation { get; set; }
        private int _foodIntake;
        private int _speed;
        public int _age;
        public int _maxAge;
        public int X { get; set; }
        public int Y { get; set; }
        private Brain _brain;
        private static Map _map;
        
        public (int, int) Position()
        {
            return (X, Y);
        }

        public static void SetMap(in Map map)
        {
            _map = map;
        }

        [JsonConstructor]
        public Animal(int id, int animalType, AnimalSex sex, int saturation, int x, int y)
        {
            Id = id;
            AnimalType = animalType;
            Sex = sex;
            Saturation = saturation;
            X = x;
            Y = y;
        }

        public Animal(int animalType, AnimalSex sex, (int, int) position)
        {
            Trace.WriteLine("Creating a " + sex + " " + Metadata.AnimalSpecifications[animalType].Name);
            Id = _currentId;
            _currentId++;
            AnimalType = animalType;
            Sex = sex;
            X = position.Item1;
            Y = position.Item2;
            _age = 0;

            // set the rest using AnimalType
            var spec = Metadata.AnimalSpecifications[animalType];
            if (spec.IsPredatory)
            {
                _brain = new PredatoryBrain();
            }
            else
            {
                _brain = new NotPredatoryBrain();
            }

            Saturation = (int) Math.Round(spec.FoodIntake * 0.4);
            _foodIntake = spec.FoodIntake;
            _speed = spec.Speed;
            _maxAge = spec.MaxAge;
            Trace.Write("The animal was born at position " + position);
            Trace.WriteLine(spec.IsPredatory ? " and is predatory" : " and is not predatory");
        }

        public void Eat(FoodSupplier food)
        {
            var typeSpec = Metadata.AnimalSpecifications[AnimalType];
            if (!typeSpec.FoodTypes.Contains(food.FoodType)) return;
            var canEat = typeSpec.FoodIntake - Saturation;
            if (canEat > food.Saturation)
            {
                canEat = food.Saturation;
            }

            food.Saturation -= canEat;
            Saturation += canEat;
        }

        private void Eat(Animal animal)
        {
            var typeSpec = Metadata.AnimalSpecifications[AnimalType];
            var canEat = typeSpec.FoodIntake - Saturation;
            if (canEat > animal._foodIntake)
            {
                canEat = animal._foodIntake;
            }

            Saturation += canEat;
        }

        private static (int, int) Fight(Animal a1, Animal a2, in Map map)
        {
            var type1 = Metadata.AnimalSpecifications[a1.AnimalType];
            var type2 = Metadata.AnimalSpecifications[a2.AnimalType];
            Animal winner, loser;

            switch (type1.IsPredatory)
            {
                case true when type2.IsPredatory:
                {
                    if (type1.FoodIntake > type2.FoodIntake)
                    {
                        winner = a1;
                        loser = a2;
                    }
                    else
                    {
                        winner = a2;
                        loser = a1;
                    }

                    break;
                }
                case true:
                    winner = a1;
                    loser = a2;
                    break;
                default:
                    winner = a2;
                    loser = a1;
                    break;
            }

            winner.Eat(loser);
            map.DeleteAnimalByPos(loser.Position());
            Trace.WriteLine(Metadata.AnimalSpecifications[loser.AnimalType].Name + " at " + loser.Position() 
                            + " has been eaten by a " + Metadata.AnimalSpecifications[winner.AnimalType].Name);
            return winner.Position();
        }

        private static bool InBounds(in Map map, (int, int) coords)
        {
            return coords.Item1 >= 0 && coords.Item1 < map.Height && coords.Item2 >= 0 && coords.Item2 < map.Width;
        }

        private static bool IsGood(in Map map, (int, int) spot)
        {
            return !map.Food.ContainsKey(spot)
                   && !map.AnimalIdByPos.ContainsKey(spot)
                   && InBounds(in _map, spot)
                   && _map.Matrix[spot.Item1, spot.Item2].BiomeId != 0;
        }
        /*
         * var spot = coords;
            var newSpot = spot;
            
            var dirAmount = 4;
            var dir = new Random().Next(dirAmount);
            var dirChangeAmount = 0;
            var step = 1 + dirChangeAmount / dirAmount;
            var isGood = false;

            do
            {
                switch (dir % dirAmount)
                {
                    case 0: //up
                        newSpot.Item1 -= step;
                        break;
                    case 1: //right
                        newSpot.Item2 += step;
                        break;
                    case 2: //down
                        newSpot.Item1 += step;
                        break;
                    case 3: //left
                        newSpot.Item2 -= step;
                        break;
                }
                if (IsGood(in _map, newSpot))
                {
                    isGood = true;
                    spot = newSpot;
                }

                newSpot = spot;
                dir++;
                dirChangeAmount++;
                step = 1 + dirChangeAmount / dirAmount;
            } while (!isGood);
            
            return spot;
         */
        internal static (int, int) FindEmptySpotNearCoords((int, int) coords)
        {
            var spot = coords;
            var newSpot = spot;
            
            var dirAmount = 4;
            var dir = new Random().Next(dirAmount);
            var dirChangeAmount = 0;
            var step = 1 + dirChangeAmount / dirAmount;
            var isGood = false;

            do
            {
                switch (dir % dirAmount)
                {
                    case 0: //up
                        newSpot.Item1 -= step;
                        break;
                    case 1: //right
                        newSpot.Item2 += step;
                        break;
                    case 2: //down
                        newSpot.Item1 += step;
                        break;
                    case 3: //left
                        newSpot.Item2 -= step;
                        break;
                }
                if (IsGood(in _map, newSpot))
                {
                    isGood = true;
                    spot = newSpot;
                }

                newSpot = spot;
                dir++;
                dirChangeAmount++;
                step = 1 + dirChangeAmount / dirAmount;
            } while (!isGood);
            
            return spot;
        }

        public void Act(in Map map)
        {
            const int hungerDecay = 1;
            Saturation -= hungerDecay;
            _age++;
            if (Saturation < 0 || _age > _maxAge)
            {
                map.DeleteAnimalByPos(Position());
                Trace.WriteLine("Animal " + Id + " " + Metadata.AnimalSpecifications[AnimalType].Name + " at " 
                                + Position() + " died of natural cause.");
                return;
            }

            // get new position
            var coords = _brain.Think(AnimalType, _foodIntake, Position(), map, DefaultFieldOfView);

            var animalIdByPos = map.AnimalIdByPos;
            var animals = map.AnimalsById;
            if (animalIdByPos.ContainsKey(coords)) // Is there an animal?
            {
                var animal = animals[animalIdByPos[coords]];
                if (animals[animalIdByPos[coords]].AnimalType == AnimalType)
                {
                    var saturationChange = (int) Math.Round(Metadata.AnimalSpecifications[AnimalType].FoodIntake * 0.8);
                    Saturation -= saturationChange;
                    animal.Saturation -= saturationChange;
                    var newCoords = FindEmptySpotNearCoords(coords);
                    map.AddAnimal(newCoords, new Animal(AnimalType,
                        new Random().Next(2) == 0 ? AnimalSex.Male : AnimalSex.Female, newCoords));
                    coords = FindEmptySpotNearCoords(coords);
                }
                else
                {
                    var winnerCoords = Fight(this, animals[animalIdByPos[coords]], in map);
                    if (winnerCoords != Position())
                    {
                        return;
                    }
                }
            }
            else
            {
                var food = map.GetFood(coords);
                if (food != null)
                {
                    Eat(food);
                    map.UpdateFood(coords);
                }
            }
            
            map.MoveAnimal(Position(), coords);
        }
    }
}