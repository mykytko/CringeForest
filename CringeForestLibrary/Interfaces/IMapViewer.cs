namespace CringeForestLibrary;

public interface IMapViewer
{
    public void AddAnimalView(Animal animal);
    public void DeleteAnimalView(int id);
    public void MoveAnimalView(int id, (int, int) coords2);
    public void SetFoodView((int, int) coords, double ratio);
    void SetInitialView(Map map);
}