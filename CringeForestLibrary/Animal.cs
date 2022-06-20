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

    internal abstract class Brain
    {
        private const double HungerThreshold = 0.1;

        protected static (int, int) ValidatePos(in Map map, (int, int) pos)
        {
            if (pos.Item1 < 0)
            {
                pos.Item1 = 0;
            } 
            else if (pos.Item1 >= map.Width)
            {
                pos.Item1 = map.Width - 1;
            }
            if (pos.Item2 < 0)
            {
                pos.Item2 = 0;
            } 
            else if (pos.Item2 >= map.Height)
            {
                pos.Item2 = map.Height - 1;
            }

            if (map.AnimalIdByPos.ContainsKey(pos))
            {
                pos = Animal.FindEmptySpotNearCoords(pos);
            }

            return pos;
        }

        public (int, int) Think(int type, int saturation, (int, int) position, in Map map, int fov)
        {
            var spec = Metadata.AnimalSpecifications[type];
            // check for predators
            // check if you're stronger
            // fight or run away
            // check if you're hungry
            // decide to search for food or for partners

            var coords = HandlePredators(type, position, in map, fov);
            if (coords != (-1, -1))
            {
                return coords;
            }
            
            var pos = Hungry(saturation, spec.FoodIntake) ? 
                LookForFood(type, position, in map, fov) : LookForPartners(type, position, in map, fov);

            if (pos == (-1, -1))
            {
                var rand = new Random();
                pos.Item1 = (rand.Next(2) == 0 ? 1 : -1) * rand.Next(2 * spec.Speed);
                pos.Item2 = (rand.Next(2) == 0 ? 1 : -1) * (spec.Speed - Math.Abs(pos.Item1));
                var k = rand.NextDouble();
                pos.Item1 = (int) Math.Floor(k * pos.Item1);
                pos.Item2 = (int) Math.Floor(k * pos.Item2);
                
                var x = pos.Item1 + position.Item1;
                var y = pos.Item2 + position.Item2;

                return ValidatePos(in map, (x, y));
            }

            var distance = Math.Sqrt(Math.Pow(pos.Item1 - position.Item1, 2) 
                                     + Math.Pow(pos.Item2 - position.Item2, 2));
            var diff = distance - spec.Speed;
            if (diff <= 0)
            {
                return pos;
            }

            var ratio = (distance + diff) / distance;
            pos.Item1 = (int) Math.Floor(pos.Item1 / ratio);
            pos.Item2 = (int) Math.Floor(pos.Item2 / ratio);
            return pos;
        }

        protected static bool Hungry(int saturation, int foodIntake)
        {
            return saturation < HungerThreshold * foodIntake;
        }

        public abstract (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov);

        public abstract (int, int) LookForFood(int type, (int, int) position, in Map map, int fov);

        protected (int, int) LookForPartners(int type, (int, int) position, in Map map, int fov)
        {
            var possiblePartners = new List<Animal>();
            for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
            {
                for (var j = position.Item1 - fov; j < position.Item1 + fov; j++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if ((j, i) == position)
                    {
                        continue;
                    }

                    if (!map.AnimalIdByPos.ContainsKey((j, i))) continue;

                    var animal = map.AnimalsById[map.AnimalIdByPos[(j, i)]];
                    if (type == animal.Type)
                    {
                        possiblePartners.Add(animal);
                    }
                }
            }

            if (possiblePartners.Count == 0)
            {
                return (-1, -1);
            }

            var closestPartner = possiblePartners[0];
            var distance = Math.Pow(position.Item1 - possiblePartners[0].X, 2)
                           + Math.Pow(position.Item2 - possiblePartners[0].Y, 2);
            foreach (var partner in possiblePartners)
            {
                var newDistance = Math.Pow(position.Item1 - partner.X, 2)
                                  + Math.Pow(position.Item2 - partner.Y, 2);
                if (newDistance >= distance) continue;
                closestPartner = partner;
                distance = newDistance;
            }

            return closestPartner.Position();
        }

        protected (int, int) FindStrongestPredator(int type, (int, int) position, in Map map, int fov)
        {
            var maxIntake = 0;
            Animal predator = null;
            for (var i = position.Item1 - fov; i < position.Item1 + fov; i++)
            {
                for (var j = position.Item2 - fov; j < position.Item2 + fov; j++)
                {
                    if (Math.Pow(position.Item1 - i, 2) + Math.Pow(position.Item2 - j, 2) > fov * fov)
                    {
                        continue;
                    }
                    
                    if ((i, j) == position)
                    {
                        continue;
                    }

                    if (!map.AnimalIdByPos.ContainsKey((i, j))) continue;
                    
                    var animal = map.AnimalsById[map.AnimalIdByPos[(i, j)]];
                    var spec = Metadata.AnimalSpecifications[animal.Type];
                    if (!spec.IsPredatory || maxIntake >= spec.FoodIntake) continue;
                    maxIntake = spec.FoodIntake;
                    predator = animal;
                }
            }

            return predator?.Position() ?? (-1, -1);
        }

        protected (int, int) FindBestFood(int type, (int, int) position, in Map map, int fov)
        {
            var maxSaturation = 0;
            var food = (-1, -1);
            for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
            {
                for (var j = position.Item1 - fov; j < position.Item1 + fov; j++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if (map.GetFood((j, i)) == null)
                    {
                        continue;
                    }
                    
                    if ((j, i) == position)
                    {
                        continue;
                    }

                    var current = map.GetFood((j, i));
                    var spec = Metadata.AnimalSpecifications[type];
                    if (current.Saturation <= maxSaturation || spec.FoodTypes.Contains(current.FoodType)) continue;
                    maxSaturation = current.Saturation;
                    food = (j, i);
                }
            }

            return food;
        }

        protected (int, int) FindBestAnimal(int type, (int, int) position, in Map map, int fov)
        {
            var maxSaturation = 0;
            Animal animal = null;
            for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
            {
                for (var j = position.Item1 - fov; j < position.Item1 + fov; j++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if (!map.AnimalIdByPos.ContainsKey((j, i)))
                    {
                        continue;
                    }
                    
                    if ((j, i) == position)
                    {
                        continue;
                    }

                    var current = map.AnimalsById[map.AnimalIdByPos[(j, i)]];
                    var spec = Metadata.AnimalSpecifications[type];
                    var hisSpec = Metadata.AnimalSpecifications[current.Type];
                    if (maxSaturation >= hisSpec.FoodIntake || hisSpec.FoodIntake > spec.FoodIntake) continue;
                    maxSaturation = hisSpec.FoodIntake;
                    animal = current;
                }
            }

            return animal?.Position() ?? (-1, -1);
        }
    }
    
    internal class PredatoryBrain : Brain
    {
        public override (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov)
        {
            var pos = FindStrongestPredator(type, position, in map, fov);
            if (pos == (-1, -1))
            {
                return pos;
            }
            
            var myStrength = Metadata.AnimalSpecifications[type].FoodIntake;
            var hisStrength = Metadata.AnimalSpecifications[map.AnimalsById[map.AnimalIdByPos[pos]].Type].FoodIntake;
            return myStrength < hisStrength ? (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2) : pos;
        }

        public override (int, int) LookForFood(int type, (int, int) position, in Map map, int fov)
        {
            var pos = FindBestAnimal(type, position, map, fov);
            if (pos == (-1, -1))
            {
                pos = FindBestFood(type, position, map, fov);
            }

            return pos;
        }
    }

    internal class NotPredatoryBrain : Brain
    {
        public override (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov)
        {
            var pos = FindStrongestPredator(type, position, in map, fov);
            if (pos == (-1, -1))
            {
                return pos;
            }
            
            var escapePos = (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2);
            return ValidatePos(in map, escapePos);
        }

        public override (int, int) LookForFood(int type, (int, int) position, in Map map, int fov)
        {
            return FindBestFood(type, position, in map, fov);
        }
    }

    public class Animal
    {
        private const int DefaultFieldOfView = 16;

        private static int _currentId = 0;
        public int Id { get; set; }
        public int Type { get; set; }
        public AnimalSex Sex { get; set; }
        public int Saturation { get; set; }
        private int _foodIntake;
        private int _speed;
        private int _age;
        private int _maxAge;
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

            Saturation = spec.FoodIntake;
            _foodIntake = spec.FoodIntake;
            _speed = spec.Speed;
            _maxAge = spec.MaxAge;
            Trace.WriteLine("The animal was born at position " + position);
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

        private static void Fight(Animal a1, Animal a2, in Map map)
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

            if (!animalIdByPos.ContainsKey(newSpot))
            {
                return newSpot;
            }

            var dir = 0;
            var step = 0;
            while (animalIdByPos.ContainsKey(spot) || spot.Item1 < 0 || spot.Item2 < 0
                   || spot.Item1 >= _map.Width || spot.Item2 >= _map.Height)
            {
                switch (dir % 4)
                {
                    case 0:
                        step++;
                        spot.Item1 += step;
                        break;
                    case 1:
                        spot.Item2 += step;
                        break;
                    case 2:
                        step++;
                        spot.Item1 -= step;
                        break;
                    case 3:
                        spot.Item2 -= step;
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
                    newCoords = FindEmptySpotNearCoords(coords);
                    map.MoveAnimal(Position(), newCoords);
                }
                else
                {
                    Fight(this, animals[animalIdByPos[coords]], in map);
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