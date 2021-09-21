using System.Collections.Generic;

namespace CringeForestLibrary
{
    public enum AnimalSex
    {
        Male,
        Female
    }
    
    public abstract class Animal
    {
        private static int _currentId;
        public int Id { get; }
        public AnimalType Type { get; }
        public List<FoodType> Foods { get; }
        public List<AnimalType> AnimalFoods { get; }
        public AnimalSex Sex { get; }
        private int _saturation;
        private int _foodIntake;
        private int _speed;
        private int _age;
        private int _maxAge;
        public (int, int) Position { get; set; }

        protected Animal(AnimalType type, AnimalSex sex, (int, int) position)
        {
            Id = _currentId;
            _currentId++;
            Type = type;
            Sex = sex;
            Position = position;
            
            // set the rest using AnimalType
        }
    }
}