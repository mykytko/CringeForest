using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public enum AnimalSex
    {
        Male,
        Female
    }
    
    public class Animal
    {
        private static int _currentId = 0;
        public int Id { get; }
        public int Type { get; }
        public AnimalSex Sex { get; }
        private int _saturation;
        private int _foodIntake;
        private int _speed;
        private int _age;
        private int _maxAge;
        public (int, int) Position { get; set; }

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
            _saturation = spec.FoodIntake;
            _foodIntake = spec.FoodIntake;
            _speed = spec.Speed;
            _maxAge = spec.MaxAge;
            Trace.WriteLine("The animal was created at position " + position);
        }

        public void Eat(FoodSupplier food)
        {
            var typeSpec = Metadata.AnimalSpecifications[Type];
            if (!typeSpec.FoodTypes.Contains(food.Type)) return;
            var canEat = typeSpec.FoodIntake - _saturation;
            if (canEat > food.Saturation)
            {
                canEat = food.Saturation;
            }

            food.Saturation -= canEat;
            _saturation += canEat;
        }

        private void Eat(Animal animal)
        {
            var typeSpec = Metadata.AnimalSpecifications[Type];
            var canEat = typeSpec.FoodIntake - _saturation;
            if (canEat > animal._foodIntake)
            {
                canEat = animal._foodIntake;
            }

            _saturation += canEat;
        }

        private (int, int) Think()
        {
            var newPosition = (1, 1);
            return newPosition;
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

        public void Act(in Map map, in Dictionary<(int, int), Animal> animals)
        {
            Trace.WriteLine("Animal " + Id + " " + Metadata.AnimalSpecifications[Type].Name + " acts");
            // get new position
            var coords = Think();

            if (animals.ContainsKey(coords)) // Is there an animal?
            {
                Fight(this, animals[coords], in map);
            }
            else
            {
                var food = map.GetFood(coords);
                if (food == null) return;
                Eat(food);
                map.UpdateFood(coords);
            }
            
            Position = coords;
            map.MoveAnimal(Position, coords);
        }
    }
}