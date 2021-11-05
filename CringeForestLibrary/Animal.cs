using System.Collections.Generic;

namespace CringeForestLibrary
{
    public enum AnimalSex
    {
        Male,
        Female
    }
    
    public class Animal
    {
        private static int _currentId;
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
        }
    }
}