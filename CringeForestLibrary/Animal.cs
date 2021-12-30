using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

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
                
                if (x < 0)
                {
                    x = 0;
                }

                if (y < 0)
                {
                    y = 0;
                }
                
                return (x, y);
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
                for (var j = position.Item1 - fov; i < position.Item1 + fov; i++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if ((i, j) == position)
                    {
                        continue;
                    }

                    if (!map.EnumerateAnimals().ContainsKey((j, i))) continue;

                    var animal = map.EnumerateAnimals()[(j, i)];
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
            var distance = Math.Pow(position.Item1 - possiblePartners[0].Position.Item1, 2)
                           + Math.Pow(position.Item2 - possiblePartners[0].Position.Item2, 2);
            foreach (var partner in possiblePartners)
            {
                var newDistance = Math.Pow(position.Item1 - partner.Position.Item1, 2)
                                  + Math.Pow(position.Item2 - partner.Position.Item2, 2);
                if (newDistance >= distance) continue;
                closestPartner = partner;
                distance = newDistance;
            }

            return closestPartner.Position;
        }

        protected (int, int) FindStrongestPredator(int type, (int, int) position, in Map map, int fov)
        {
            var maxIntake = 0;
            Animal predator = null;
            for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
            {
                for (var j = position.Item1 - fov; i < position.Item1 + fov; i++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }
                    
                    if ((i, j) == position)
                    {
                        continue;
                    }

                    if (!map.EnumerateAnimals().ContainsKey((j, i))) continue;

                    var animal = map.EnumerateAnimals()[(j, i)];
                    var spec = Metadata.AnimalSpecifications[animal.Type];
                    if (spec.IsPredatory && maxIntake < spec.FoodIntake)
                    {
                        maxIntake = spec.FoodIntake;
                        predator = animal;
                    }
                }
            }

            return predator?.Position ?? (-1, -1);
        }

        protected (int, int) FindBestFood(int type, (int, int) position, in Map map, int fov)
        {
            var maxSaturation = 0;
            (int, int) food = (-1, -1);
            for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
            {
                for (var j = position.Item1 - fov; i < position.Item1 + fov; i++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if (map.GetFood((i, j)) == null)
                    {
                        continue;
                    }
                    
                    if ((i, j) == position)
                    {
                        continue;
                    }

                    var current = map.GetFood((i, j));
                    var spec = Metadata.AnimalSpecifications[type];
                    if (current.Saturation <= maxSaturation || spec.FoodTypes.Contains(current.Type)) continue;
                    maxSaturation = current.Saturation;
                    food = (i, j);
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
                for (var j = position.Item1 - fov; i < position.Item1 + fov; i++)
                {
                    if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                    {
                        continue;
                    }

                    if (!map.EnumerateAnimals().ContainsKey((i, j)))
                    {
                        continue;
                    }
                    
                    if ((i, j) == position)
                    {
                        continue;
                    }

                    var current = map.EnumerateAnimals()[(i, j)];
                    var spec = Metadata.AnimalSpecifications[type];
                    var hisSpec = Metadata.AnimalSpecifications[current.Type];
                    if (maxSaturation >= hisSpec.FoodIntake || hisSpec.FoodIntake > spec.FoodIntake) continue;
                    maxSaturation = hisSpec.FoodIntake;
                    animal = current;
                }
            }

            return animal?.Position ?? (-1, -1);
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
            var hisStrength = Metadata.AnimalSpecifications[map.EnumerateAnimals()[pos].Type].FoodIntake;
            return myStrength < hisStrength ? (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2) : pos;
        }

        public override (int, int) LookForFood(int type, (int, int) position, in Map map, int fov)
        {
            return FindBestFood(type, position, in map, fov);
        }
    }

    internal class NotPredatoryBrain : Brain
    {
        public override (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov)
        {
            var pos = FindStrongestPredator(type, position, in map, fov);
            return pos == (-1, -1) ? pos : (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2);
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

    public class Animal
    {
        private const int DefaultFieldOfView = 16;

        private static int _currentId = 0;
        public int Id { get; }
        public int Type { get; }
        public AnimalSex Sex { get; }
        public int Saturation { get; set; }
        private int _foodIntake;
        private int _speed;
        private int _age;
        private int _maxAge;
        public (int, int) Position { get; set; }
        private Brain _brain;
        private static Map _map;

        public static void SetMap(in Map map)
        {
            _map = map;
        }

        public Animal(int type, AnimalSex sex, (int, int) position)
        {
            Trace.WriteLine("Creating a " + sex + " " + Metadata.AnimalSpecifications[type].Name);
            Id = _currentId;
            _currentId++;
            Type = type;
            Sex = sex;
            Position = position;
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
            Trace.WriteLine("The animal was created at position " + position);
        }

        public void Eat(FoodSupplier food)
        {
            var typeSpec = Metadata.AnimalSpecifications[Type];
            if (!typeSpec.FoodTypes.Contains(food.Type)) return;
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
            map.DeleteAnimal(loser.Position);
        }

        private (int, int) FindEmptySpotNearCoords((int, int) coords)
        {
            var spot = coords;
            var animals = _map.EnumerateAnimals();
            
            while (animals.ContainsKey(spot))
            {
                spot.Item1++;
            }

            return spot;
        }

        public void Act(in Map map, in ConcurrentDictionary<(int, int), Animal> animals)
        {
            const int HungerDecay = 1;
            Trace.WriteLine("Animal " + Id + " " + Position 
                            + " " + Metadata.AnimalSpecifications[Type].Name + " acts");
            // get new position
            var coords = _brain.Think(Type, _foodIntake, Position, map, DefaultFieldOfView);

            Saturation -= HungerDecay;
            if (Saturation <= 0)
            {
                map.DeleteAnimal(Position);
                return;
            }

            if (animals.ContainsKey(coords)) // Is there an animal?
            {
                if (animals[coords].Type == Type)
                {
                    var newCoords = FindEmptySpotNearCoords(coords);
                    map.AddAnimal(newCoords, new Animal(Type,
                        new Random().Next(2) == 0 ? AnimalSex.Male : AnimalSex.Female, newCoords));
                    newCoords = FindEmptySpotNearCoords(coords);
                    Position = newCoords;
                    map.MoveAnimal(Position, coords);
                }
                Fight(this, animals[coords], in map);
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

            Position = coords;
            map.MoveAnimal(Position, coords);
            Trace.WriteLine("Animal " + Id + " " + Metadata.AnimalSpecifications[Type].Name + " acted");
        }
    }
}