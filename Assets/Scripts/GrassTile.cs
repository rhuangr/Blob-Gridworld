
public class GrassTile : Tile
{
    private readonly int ENERGY_COST = 1;

    public GrassTile(int x, int y) : base(x ,y){
        energyCost = ENERGY_COST;
        isWalkable = true;
    }
}
