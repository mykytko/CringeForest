using System;
using System.Collections.Generic;

namespace CringeForestLibrary;

internal abstract partial class Brain
{
    //BTW: these thresholds should be adjusted manually
    //so that the simulation runs as long and dynamically as possible
    //Thresholds should be moved to specification.json
    //and have different value for different animals
    private const double HungerThreshold = 0.4;
    private const double FullThreshold = 0.6;

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

        if (map.AnimalIdByPos.ContainsKey(pos) || map.Matrix[pos.Item1, pos.Item2].BiomeId == 0)
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

        (int, int) pos;
        if (Hungry(saturation, spec.FoodIntake))
        {
            pos = LookForFood(type, position, in map, fov);
        } else if (Full(saturation, spec.FoodIntake))
        {
            pos = LookForPartners(type, position, in map, fov);
        }
        else
        {
            pos = Wander(position, in map, spec);
        }

        var distance = Math.Sqrt(Math.Pow(pos.Item1 - position.Item1, 2)
                                 + Math.Pow(pos.Item2 - position.Item2, 2));
        var diff = distance - spec.Speed;
        if (diff <= 0)
        {
            return ValidatePos(in map, pos);
        }

        var ratio = (distance + diff) / distance;
        pos.Item1 = (int) Math.Floor(pos.Item1 / ratio);
        pos.Item2 = (int) Math.Floor(pos.Item2 / ratio);
        return ValidatePos(in map, pos);
    }

    private static (int, int) Wander((int, int) position, in Map map, AnimalSpecification spec)
    {
        (int, int) pos;
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

    protected static bool Hungry(int saturation, int foodIntake)
    {
        return saturation < HungerThreshold * foodIntake;
    }

    private static bool Full(int saturation, int foodIntake)
    {
        return saturation > FullThreshold * foodIntake;
    }

    public abstract (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov);
    public abstract (int, int) LookForFood(int type, (int, int) position, in Map map, int fov);

    protected (int, int) LookForPartners(int type, (int, int) position, in Map map, int fov)
    {
        var currentAnimal = map.AnimalsById[map.AnimalIdByPos[position]];
        if (currentAnimal._age * 2 >= currentAnimal._maxAge)
        {
            return (-1, -1);
        }

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
                if (animal._age * 2 >= animal._maxAge)
                {
                    continue;
                }

                if (type == animal.AnimalType)
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

    protected (int, int) FindClosestPredator(int type, (int, int) position, in Map map, int fov)
    {
        var minDistance = double.MaxValue;
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
                var spec = Metadata.AnimalSpecifications[animal.AnimalType];
                var distance = Math.Sqrt(Math.Pow(j - position.Item2, 2) + Math.Pow(i - position.Item1, 2));
                if (!spec.IsPredatory || minDistance <= distance) continue;
                minDistance = distance;
                predator = animal;
            }
        }

        return predator?.Position() ?? (-1, -1);
    }

    protected (int, int) FindClosestFood(int type, (int, int) position, in Map map, int fov)
    {
        var minDistance = double.MaxValue;
        var food = (-1, -1);
        for (var i = position.Item2 - fov; i < position.Item2 + fov; i++)
        {
            for (var j = position.Item1 - fov; j < position.Item1 + fov; j++)
            {
                if (Math.Pow(position.Item2 - i, 2) + Math.Pow(position.Item1 - j, 2) > fov * fov)
                {
                    continue;
                }

                if (map.AnimalIdByPos.ContainsKey((j, i)))
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
                var distance = Math.Sqrt(Math.Pow(j - position.Item1, 2) + Math.Pow(i - position.Item2, 2));
                if (minDistance <= distance || !spec.FoodTypes.Contains(current.FoodType)) continue;
                minDistance = distance;
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
                var hisSpec = Metadata.AnimalSpecifications[current.AnimalType];
                if (maxSaturation >= hisSpec.FoodIntake || hisSpec.FoodIntake > spec.FoodIntake) continue;
                maxSaturation = hisSpec.FoodIntake;
                animal = current;
            }
        }

        return animal?.Position() ?? (-1, -1);
    }
}