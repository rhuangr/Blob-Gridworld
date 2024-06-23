using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgentBrain : MonoBehaviour
{
    public Tile [][] world {get; private set;}
    public Tile start {get; private set;}
    public Tile currentTile;
    Tile predTile;
    Tile appleTile;
    float epsilon = 0.6f;
    float gamma = 1;
    float alpha = 0.5f;
    public int currentLevel {get; private set;}

    string[] possibleActions = {"up","down","left","right"};
    System.Random random = new System.Random();

    Dictionary<StateAction, float> SAV= new Dictionary<StateAction, float>();
    Dictionary<StateAction, Dictionary<StateAction, int>> model = new Dictionary<StateAction, Dictionary<StateAction, int>>();
    // Start is called before the first frame update
    public void Awake()
    {

        currentLevel = SceneManager.GetActiveScene().buildIndex + 1;
        world = LevelsDatabase.getLevel(currentLevel);
        start = world[0][world[0].Length-1]; //world[0].Length-1
        currentTile = start;
        int[] predPos = LevelsDatabase.getPredPos(currentLevel);
        int[] applePos = LevelsDatabase.getApplePos(currentLevel);
        LevelsDatabase.setApples(currentLevel, world);
        appleTile = world[applePos[0]][applePos[1]];
        if (predPos == null){
            predTile = new GrassTile(-2, -2);
        }

        setStateActionValues();
    }

    private void setStateActionValues()
    {
        for (int i = 0; i < world.Length; i++){
            for (int j = 0; j < world[i].Length; j++){
                SAV[new StateAction(world[i][j], predTile, appleTile, 0)] = 0;
                SAV[new StateAction(world[i][j], predTile, appleTile, 1)] = 0;
                SAV[new StateAction(world[i][j], predTile, appleTile, 2)] = 0;
                SAV[new StateAction(world[i][j], predTile, appleTile, 3)] = 0;
            }
        }
    }
    public void train(int episodes, int planningSteps){
        int episodeCount = 0;
        for (int i=0 ; i<episodes; i++){
            episodeCount++;
            int moves = (getEpisode(planningSteps));
            // Debug.Log(moves);
            
            if (epsilon > 0.1f){
                epsilon = epsilon/episodeCount;
            }
            currentTile = start;
            LevelsDatabase.setApples(currentLevel, world);
        }
    }
    private int getEpisode(int planningSteps){
        int reward;
        int actionsCount = 0;

        while (!currentTile.hasApple){
            // actionsCount += 1;
            if (actionsCount == 2000){break;}
            int actionIndex = getMaxAction(false);
            // debug.log("TAKEN:" + possibleActions[actionIndex] +"count:" + test);
            StateAction currentSA = getStateAction(actionIndex);

            reward = -1 * currentTile.energyCost;
            // Debug.Log(actionIndex);
            if (takeAction(actionIndex)){
                checkIce(actionIndex); 
            }
            
            if (currentTile.hasApple){
                reward = 0;
            }
            
            
            int argmaxA = getMaxAction(true);   
            StateAction nextSA = getStateAction(argmaxA);
            // PREDATOR ACTS HERE IMPLEMENT LATER 
            float prevValue = SAV[currentSA];
            float newValue;
            newValue = prevValue + alpha*(reward + gamma*SAV[nextSA] - prevValue);
            
            
            // if (newValue < -5){
                // Debug.Log("action: " + possibleActions[actionIndex]+currentSA.preyPosition.posX +", "+currentSA.preyPosition.posY + " NEXT:" + nextSA.preyPosition.posX +", "+nextSA.preyPosition.posY);
                // Debug.Log(prevValue +" + " +alpha+" *("+reward +" + "+ SAV[nextSA] + " - "+ prevValue +" = "+newValue);
            // }
            // Debug.Log(prevValue +" + " +alpha+" *("+reward +" + "+ SAV[nextSA] + " - "+ prevValue +" = "+newValue);
            SAV[currentSA] = newValue;

            model[currentSA] = new Dictionary<StateAction, int>
            {{nextSA, reward}};
            List<StateAction> keys = new List<StateAction>(model.Keys);
            for (int j=0 ; j < planningSteps; j++){
                int randomIndex = random.Next(keys.Count);
                StateAction sampledSA = keys[randomIndex];
                Dictionary<StateAction, int> SAVReward = model[sampledSA];
                StateAction sampledSAPrime = SAVReward.Keys.First();
                int sampleReward = SAVReward[sampledSAPrime];
                float plannedSAV = SAV[sampledSA] + alpha*(sampleReward + gamma*getMaxActionValue(sampledSAPrime) - SAV[sampledSA]);
                // if (plannedSAV < -5){
                //     Debug.Log("ACTION: " + sampledSA.action +" "+ sampledSA.preyPosition.posX +", "+sampledSA.preyPosition.posY + " NEXT:" + sampledSAPrime.preyPosition.posX +", "+sampledSAPrime.preyPosition.posY);
                //     Debug.Log(SAV[sampledSA]+" + " +"("+sampleReward +" + "+ getMaxActionValue(sampledSAPrime) + " - "+ SAV[sampledSA] +" = "+plannedSAV);
                // }
                SAV[sampledSA] = plannedSAV;
            }
        }
        return actionsCount;
    }

    public bool checkIce( int actionIndex){
        if (!(currentTile is IceTile)){
            return false;
        }
        if (random.Next(0,2) == 1){
            // Debug.Log($"before slipping:{currentTile.xIndex} and {currentTile.yIndex}");
            while (true){
                if (!takeAction(actionIndex) || !(currentTile is IceTile)){
                    // Debug.Log($"after slipping: {currentTile.xIndex} and {currentTile.yIndex}");
                    return true;
                }

            }
        }
        return false;
    }


    private int getMaxAction(bool getMax){

        float[] actionValues = new float[4];

        actionValues[0] = SAV[getStateAction(0)];
        actionValues[1] = SAV[getStateAction(1)];
        actionValues[2] = SAV[getStateAction(2)];
        actionValues[3] = SAV[getStateAction(3)];

        if (getMax){
            // Debug.Log(Array.IndexOf(actionValues, actionValues.Max()));
            return Array.IndexOf(actionValues, actionValues.Max());
        }
        Double epsilonCheck = random.NextDouble();
        int action;
        if (epsilonCheck > epsilon){
            action = Array.IndexOf(actionValues, actionValues.Max());
        }
        else{action = random.Next(4);}
        // Debug.Log("ACTIONS:" + string.Join(", ", validActions));
        // Debug.Log(action);
        return action;
    }

    private float getMaxActionValue(StateAction state){

        float[] actionValues = new float[4];

        actionValues[0] = SAV[getStateAction(0, state)];
        actionValues[1] = SAV[getStateAction(1, state)];
        actionValues[2] = SAV[getStateAction(2, state)];
        actionValues[3] = SAV[getStateAction(3, state)];

        return actionValues.Max();
    }

    private bool takeAction(int actionIndex){
        bool xIsValidPositive = currentTile.xIndex + 1 < world.Length;
        bool xIsValidNegative = currentTile.xIndex - 1 > -1;
        bool yIsValidPositive = currentTile.yIndex + 1 < world[0].Length;
        bool yIsValidNegative = currentTile.yIndex - 1 > -1;

        if (actionIndex == 0 && yIsValidPositive && world[currentTile.xIndex][currentTile.yIndex + 1].isWalkable){ // move up
            currentTile = world[currentTile.xIndex][currentTile.yIndex + 1];
            return true;
        }
        else if (actionIndex == 1 && yIsValidNegative && world[currentTile.xIndex][currentTile.yIndex-1].isWalkable){
            // Debug.Log("" + currentTile.posX + (currentTile.posY-1));
            currentTile = world[currentTile.xIndex][currentTile.yIndex - 1];
            return true;
        }
        else if (actionIndex == 2 && xIsValidNegative && world[currentTile.xIndex-1][currentTile.yIndex].isWalkable){
            currentTile = world[currentTile.xIndex - 1][currentTile.yIndex];
             return true;
        }
        else if (actionIndex == 3 && xIsValidPositive && world[currentTile.xIndex+1][currentTile.yIndex].isWalkable){
            currentTile = world[currentTile.xIndex + 1][currentTile.yIndex];
            return true;
        }
        return false;
    }

    private StateAction getStateAction(int actionIndex){
        return new StateAction(currentTile, predTile, appleTile, actionIndex);
    }

    private StateAction getStateAction( int actionIndex, StateAction state){
        return new StateAction(state.preyPosition, state.predatorPosition, state.applePosition, actionIndex);
    }

    private int getReward(){
        if (currentTile.hasApple){
            return 1;
        }
        return 0;
    }
    public List<int> getBestPath(Tile currentTile){
        this.currentTile = currentTile;
        List<int> movesList = new List<int>();
        int turns = 0;
        while (!currentTile.hasApple){
            turns++;
            int i = getMaxAction(true);
            movesList.Add(i);
            takeAction(i);
            // Debug.Log($"took action {i}"); 
            if (checkIce(i)){Debug.Log("SLIPPERY!!!");break;}
            if (turns == 20){
                break;
            }
            // Debug.Log("turn:"+turns +"curPos:"+currentTile.posX +", " + currentTile.posY+" actionTaken:" + possibleActions[i]);
        }
        // Debug.Log($"{currentTile.xIndex} and {currentTile.yIndex} and {currentTile.hasApple}");
        // Debug.Log(turns);
        return movesList;
    }
    
}
    // private int getMaxAction(bool getMax){

    //     float[] actionValues = new float[4];
    //     bool xIsValidPositive = currentTile.posX + 1 < world.Length;
    //     bool xIsValidNegative = currentTile.posX - 1 > -1;
    //     bool yIsValidPositive = currentTile.posY + 1 < world[0].Length;
    //     bool yIsValidNegative = currentTile.posY - 1 > -1;

    //     // Debug.Log("xPos:" + xIsValidPositive +"Xneg:" +xIsValidNegative);
    //     // Debug.Log("yPos:" +yIsValidPositive +"yNeg:"+yIsValidNegative);
    //     //debug.log("POS:" + currentTile.posX+"," +currentTile.posY);
    //     List<int> validActions = new List<int>();

    //     if (yIsValidPositive && world[currentTile.posX][currentTile.posY + 1].isWalkable){
    //         actionValues[0] = SAV[GetStateAction(0)];
    //         validActions.Add(0);     
    //     }
    //     else{actionValues[0] = -1000000;}

    //     if (yIsValidNegative && world[currentTile.posX][currentTile.posY-1].isWalkable){
    //         actionValues[1] = SAV[GetStateAction(1)];
    //         validActions.Add(1);
    //     }
    //     else {actionValues[1] = -1000000;}

    //     if (xIsValidNegative && world[currentTile.posX-1][currentTile.posY].isWalkable){
    //         actionValues[2] = SAV[GetStateAction(2)];
    //         validActions.Add(2);
    //     }
    //     else{actionValues[2] = -1000000;}

    //     if (xIsValidPositive && world[currentTile.posX+1][currentTile.posY].isWalkable){
    //         actionValues[3] = SAV[GetStateAction(3)];
    //         validActions.Add(3);
    //     }
    //     else{actionValues[3] = -1000000;}

    //     if (getMax){
    //         // Debug.Log(Array.IndexOf(actionValues, actionValues.Max()));
    //         return Array.IndexOf(actionValues, actionValues.Max());
    //     }
    //     Double epsilonCheck = random.NextDouble();
    //     int action;
    //     if (epsilonCheck > epsilon){
    //         action = Array.IndexOf(actionValues, actionValues.Max());

    //     }
    //     else{
    //         int index = random.Next(validActions.Count);
    //         action = validActions[index];
    //     }
    //     // Debug.Log("ACTIONS:" + string.Join(", ", validActions));
    //     // Debug.Log(action);
    //     return action;
    // }
    
    // private void takeAction(int actionIndex){
    //     if (actionIndex == 0){ // move up
    //         currentTile = world[currentTile.posX][currentTile.posY + 1];
    //     }
    //     else if (actionIndex == 1){
    //         // Debug.Log("" + currentTile.posX + (currentTile.posY-1));
    //         currentTile = world[currentTile.posX][currentTile.posY - 1];
    //     }
    //     else if (actionIndex == 2){
    //         currentTile = world[currentTile.posX - 1][currentTile.posY];
    //     }
    //     else if (actionIndex == 3){
    //         currentTile = world[currentTile.posX + 1][currentTile.posY];
    //     }
    // }
