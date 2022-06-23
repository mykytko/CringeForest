namespace CringeForestLibrary;

internal partial class PredatoryBrain
{
    public override (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov)
    {
        var pos = FindClosestPredator(type, position, in map, fov);
        if (pos == (-1, -1))
        {
            return pos;
        }

        var myStrength = Metadata.AnimalSpecifications[type].FoodIntake;
        var hisStrength = Metadata.AnimalSpecifications[map.AnimalsById[map.AnimalIdByPos[pos]].AnimalType].FoodIntake;
        return myStrength < hisStrength ? 
            ValidatePos(in map, (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2)) : pos;
    }

    public override (int, int) LookForFood(int type, (int, int) position, in Map map, int fov)
    {
        var pos = FindBestAnimal(type, position, map, fov);
        if (pos == (-1, -1))
        {
            pos = FindClosestFood(type, position, map, fov);
        }

        return pos;
    }
}