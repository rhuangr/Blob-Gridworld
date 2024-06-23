
public class IceTile : Tile
{
    public readonly int ENERGY_COST = 1;

    public IceTile(int x, int y): base(x,y){
        isWalkable = true;
        energyCost = ENERGY_COST;
    }
}
