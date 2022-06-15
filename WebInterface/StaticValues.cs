using CringeForestLibrary;

namespace WebInterface;

public static class StaticValues
{
    public static CringeForest CringeForest = new CringeForest(new MapViewer(new MapHub()));
}