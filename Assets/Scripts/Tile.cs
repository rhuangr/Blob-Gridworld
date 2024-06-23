
public abstract class Tile
{
    public int energyCost {get; protected set; }
    public bool hasApple { get; set; }
    public bool isOccupied { get; set; }
    public int xIndex {get;}
    public int yIndex {get;}

    public bool isWalkable {get; protected set;}

    public Tile( int posX, int posY){
       this.xIndex = posX;
       this.yIndex = posY;
    }
}
