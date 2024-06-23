
public static class LevelsDatabase 
{
    public static Tile[][] getLevel(int n){
        switch(n){
            case 1:
                return setTiles(2,4);
            case 2:
                return setTiles(5,5);
            case 3:
                return getLevelThree();
            case 4: 
                return getLevelFour();
            case 5:
                return getLevelFive();
            case 6:
                return getLevelSix();
            default:
                return setTiles(5,5);
        }
    }

    public static int[]  getApplePos(int n){


        switch(n){
            case 1:
                return new int[] { 1, 0 };
            case 2:
                return new int[] { 4, 2 };
            case 3: 
                return new int[] { 2, 0 };
            case 4:
                return new int[] {2,0};
            case 5:
                return new int[] {1,2};
            case 6:
                return new int[] {0,2};
            default:
                return new int[] { 2, 0 }; 
        }
    }
    public static int[]  getPredPos(int n){

        switch(n){
            default:
                return null;
        }
    }
    private static Tile[][] getLevelThree(){
        Tile[][] level = setTiles(3,3);
        level[0][1] = new MudTile(0,1);
        level[1][1] = new MudTile(1,1);
        return level;
    }
    private static Tile[][] getLevelFour(){
        Tile[][] level = setTiles(4,5);
        int[][] mudTiles = {new int[] {0,2}, new int[] {1,1},new int[] {1,3},new int[] {3,2}};
        int[][] noTiles = {new int[] {0,0},new int[] {0,1},new int[] {2,1}};
        setTiles("mud", mudTiles, level);
        setTiles("none",noTiles, level);
        return level;
    }
    private static Tile[][] getLevelFive(){
        Tile[][] level = setTiles(3,5);
        int[][] iceTiles = {new int[] {0,0},new int[] {0,1},new int[] {0,2},new int[] {0,3},new int[]{1,4}, new int[]{2,4}};
        int[][] noTiles = {new int[]{1,3}};   
        setTiles("ice", iceTiles, level);
        setTiles("none", noTiles, level);
        return level; 
    }
    private static Tile[][] getLevelSix(){
        Tile[][] level = setTiles(3,6);
        int[][] iceTiles = {new int[] {0,1},new int[] {0,2},new int[] {0,3},new int[] {0,4},new int[]{1,2}, new int[]{1,5},
         new int[]{2,2}, new int[]{2,5}};
        int[][] mudTiles = {new int[]{0,0}, new int[]{1,3}};   
        setTiles("ice", iceTiles, level);
        setTiles("mud", mudTiles, level);
        return level; 
    }


    public static void setApples(int level, Tile[][] world){
        int[] applePos = getApplePos(level);
        world[applePos[0]][applePos[1]].hasApple = true;
        
    }

    public static Tile[][] setTiles( int rows, int columns){
        Tile[][] level = new Tile[rows][];
        for (int i = 0 ; i < rows; i++){
            level[i] = new Tile[columns];
            for (int j = 0; j<columns ;j++){
                level[i][j] = new GrassTile(i, j);
            }
        }
        return level;
    }
    public static void setTiles(string type, int[][] tilePos, Tile[][] level){
        if (type == "mud"){
            foreach (int[] pos in tilePos){
                level[pos[0]][pos[1]] = new MudTile(pos[0], pos[1]);
            }
        }
        else if (type == "none"){
            foreach (int[] pos in tilePos){
                level[pos[0]][pos[1]] = new NoTile(pos[0], pos[1]);
            }
        }
        else if (type == "ice"){
            foreach (int[] pos in tilePos){
                level[pos[0]][pos[1]] = new IceTile(pos[0], pos[1]);
            }
        }
    }
}
