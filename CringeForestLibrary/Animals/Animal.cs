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
        private const int DefaultFieldOfView = 8;

        private static int _currentId = 0;
        public int Id { get; set; }
        public int Type { get; set; }
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
        public Animal(int id, int type, AnimalSex sex, int saturation, int x, int y)
        {
            Id = id;
            Type = type;
            Sex = sex;
            Saturation = saturation;
            X = x;
            Y = y;
        }

        public Animal(int type, AnimalSex sex, (int, int) position)
        {
            Trace.WriteLine("Creating a " + sex + " " + Metadata.AnimalSpecifications[type].Name);
            Id = _currentId;
            _currentId++;
            Type = type;
            Sex = sex;
            X = position.Item1;
            Y = position.Item2;
            _age = 0;

            // set the rest using AnimalType
            var spec = Metadata.AnimalSpecifications[type];
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
            var typeSpec = Metadata.AnimalSpecifications[Type];
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
            var typeSpec = Metadata.AnimalSpecifications[Type];
            var canEat = typeSpec.FoodIntake - Saturation;
            if (canEat > animal._foodIntake)
            {
                canEat = animal._foodIntake;
            }

            Saturation += canEat;
        }

        private static (int, int) Fight(Animal a1, Animal a2, in Map map)
        {
            var type1 = Metadata.AnimalSpecifications[a1.Type];
            var type2 = Metadata.AnimalSpecifications[a2.Type];
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
            Trace.WriteLine(Metadata.AnimalSpecifications[loser.Type].Name + " at " + loser.Position() 
                            + " has been eaten by a " + Metadata.AnimalSpecifications[winner.Type].Name);
            return winner.Position();
        }

        private static bool InBounds(in Map map, (int, int) coords)
        {
            return coords.Item1 >= 0 && coords.Item1 < map.Width && coords.Item2 >= 0 && coords.Item2 < map.Height;
        }

        private static bool IsGood(in Map map, (int, int) spot)
        {
            return !map.Food.ContainsKey(spot)
                   && !map.AnimalIdByPos.ContainsKey(spot)
                   && InBounds(in _map, spot)
                   && _map.Matrix[spot.Item1, spot.Item2].BiomeId != 0;
        }

        internal static (int, int) FindEmptySpotNearCoords((int, int) coords)
        {
            var spot = coords;
            var animalIdByPos = _map.AnimalIdByPos;

            var coord = new Random().Next(2);
            var sign = new Random().Next(2);
            if (sign == 0)
            {
                sign = -1;
            }
            
            var newSpot = spot;
            if (coord == 0)
            {
                newSpot.Item1 += sign;
            }
            else
            {
                newSpot.Item2 += sign;
            }

            if (InBounds(in _map, newSpot) && !animalIdByPos.ContainsKey(newSpot) 
                                  && _map.Matrix[newSpot.Item1, newSpot.Item2].BiomeId != 0)
            {
                return newSpot;
            }

            var dir = 0;
            var step = 0;
            var isGood = false;
            while (!IsGood(in _map, spot))
            {
                switch (dir % 4)
                {
                    case 0:
                        step++;
                        var limit = spot.Item1 + step;
                        while (spot.Item1 < limit)
                        {
                            if (IsGood(in _map, spot))
                            {
                                isGood = true;
                                break;
                            }
                            spot.Item1++;
                        }
                        break;
                    case 1:
                        limit = spot.Item2 + step;
                        while (spot.Item2 < limit)
                        {
                            if (IsGood(in _map, spot))
                            {
                                isGood = true;
                                break;
                            }
                            spot.Item2++;
                        }
                        break;
                    case 2:
                        step++;
                        limit = spot.Item1 - step;
                        while (spot.Item1 > limit)
                        {
                            if (IsGood(in _map, spot))
                            {
                                isGood = true;
                                break;
                            }
                            spot.Item1--;
                        }
                        break;
                    case 3:
                        limit = spot.Item2 - step;
                        while (spot.Item2 > limit)
                        {
                            if (IsGood(in _map, spot))
                            {
                                isGood = true;
                                break;
                            }
                            spot.Item2--;
                        }
                        break;
                }

                if (isGood)
                {
                    break;
                }
                dir++;
            }

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
                Trace.WriteLine("Animal " + Id + " " + Metadata.AnimalSpecifications[Type].Name + " at " 
                                + Position() + " died of natural cause.");
                return;
            }

            // get new position
            var coords = _brain.Think(Type, _foodIntake, Position(), map, DefaultFieldOfView);

            var animalIdByPos = map.AnimalIdByPos;
            var animals = map.AnimalsById;
            if (animalIdByPos.ContainsKey(coords)) // Is there an animal?
            {
                var animal = animals[animalIdByPos[coords]];
                if (animals[animalIdByPos[coords]].Type == Type)
                {
                    var saturationChange = (int) Math.Round(Metadata.AnimalSpecifications[Type].FoodIntake * 0.8);
                    Saturation -= saturationChange;
                    animal.Saturation -= saturationChange;
                    var newCoords = FindEmptySpotNearCoords(coords);
                    map.AddAnimal(newCoords, new Animal(Type,
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