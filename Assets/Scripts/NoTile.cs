
public class NoTile : Tile
{
    public readonly int ENERGY_COST = 0;
    public NoTile(int x, int y) : base(x ,y)
    {
        energyCost = ENERGY_COST;
        isWalkable = false;
    }
}
