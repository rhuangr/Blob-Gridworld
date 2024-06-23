
public class MudTile : Tile
{
    public readonly int ENERGY_COST = 2;
    public MudTile(int x, int y) : base(x ,y)
    {
        energyCost = ENERGY_COST;
        isWalkable = true;
    }
}
