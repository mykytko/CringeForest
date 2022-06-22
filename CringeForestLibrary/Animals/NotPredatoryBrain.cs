using System;

namespace CringeForestLibrary;

internal partial class NotPredatoryBrain
{
    public override (int, int) HandlePredators(int type, (int, int) position, in Map map, int fov)
    {
        var pos = FindClosestPredator(type, position, in map, fov);
        if (pos == (-1, -1))
        {
            return pos;
        }
        
        var escapePos = (2 * position.Item1 - pos.Item1, 2 * position.Item2 - pos.Item2);
        return ValidatePos(in map, escapePos);
    }

    public override (int, int) LookForFood(int type, (int, int) position, in Map map, int fov)
    {
        return FindClosestFood(type, position, in map, fov);
    }
}