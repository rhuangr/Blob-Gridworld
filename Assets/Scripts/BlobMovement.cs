using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BlobMovement : MonoBehaviour
{
    Transform pos;
    Animator myAnimator;
    AgentBrain agent;
    Tile[][] world;
    Tile currentTile;
    string currentAnimation;
    [Header("blobSpeed")]
    [SerializeField] float xPosChange;
    [SerializeField] float yPosChange;
    [SerializeField] float speed;

    Button trainButton;
    GameObject nextLevelButton;
    float originalSpeed;
    LineRenderer lr;
    GameObject connectorObject;
    Vector3[] linePos;
    Vector3 currentPoint;
    Tilemap tilemap;
    ConnectTiles connector;
    Coroutine mudCoroutine;
    private bool isSlipping;
    bool isMoving;


    void Start()
    {
        
        myAnimator = GetComponent<Animator>();
        connectorObject = GameObject.Find("TileConnector");
        lr = connectorObject.GetComponent<LineRenderer>();
        agent = FindObjectOfType<AgentBrain>();
        tilemap = FindObjectOfType<Tilemap>();
        connector = FindObjectOfType<ConnectTiles>();
        trainButton = GameObject.Find("train").GetComponent<Button>();
        world = agent.world;
        pos = transform;
        originalSpeed = speed;
        currentAnimation = "isIdle";
        
        currentTile = agent.start;
        // Debug.Log($"{currentTile.xIndex} and {currentTile.yIndex}");
    }

    // Update is called once per frame
    void Update()
    {
        if  (!isMoving && Input.GetKeyDown(KeyCode.Space)){
            startPath();
        }
    }
    public void restart(){
        SceneManager.LoadScene(agent.currentLevel - 1);
    }
    public void startPath(int episodes){
        StartCoroutine(startPathCoroutine(episodes));
    }
    public IEnumerator startPathCoroutine(int episodes){
        agent.train(episodes, 25);
        List<int> bestPath = agent.getBestPath(currentTile);
        yield return StartCoroutine(takePath(bestPath));
        // int count = 0;
        int lastSlip = bestPath[bestPath.Count - 1];
        while(true){
            // count++;
            // if (count == 3){
            //     Debug.Log("oh no!");
            //     break;
            // }
            if (currentTile.hasApple){
                break;
            }
            int xDif = math.abs(currentTile.xIndex - agent.currentTile.xIndex);
            int yDif = math.abs(currentTile.yIndex - agent.currentTile.yIndex);
            int max = math.max(xDif, yDif);
            speed = originalSpeed*4;
            for (int i=0; i<max; i++){
                yield return StartCoroutine(moveBlob(lastSlip));
            }
            Debug.Log(max);
            speed=originalSpeed;
            bestPath = agent.getBestPath(currentTile);
            // Debug.Log(string.Join(", ", bestPath));
            yield return StartCoroutine(takePath(bestPath));
            // Debug.Log(lastSlip);
            if (bestPath.Count != 0){
                lastSlip = bestPath[bestPath.Count - 1];
            }
        }
    }
    public void startPath(){
        StartCoroutine(followLine());
    }
    private IEnumerator takePath(List<int> actionsList){
        if (actionsList.Count > 20){
            showLoopWarning();
        }
        foreach (int actionIndex in actionsList){
            yield return StartCoroutine(moveBlob(actionIndex));
        }
        if (currentTile.hasApple){
            myAnimator.SetBool(currentAnimation, false);
            myAnimator.SetBool("isIdle", true);
            myAnimator.SetBool("isHappy", true);
        }
    }
    public IEnumerator moveBlob(int actionIndex){
        if (actionIndex == 0){
            setAnimation(0);
            yield return StartCoroutine(moveBlobCoroutine(1,1));
        }
        else if (actionIndex == 1){
            setAnimation(1);
            yield return StartCoroutine(moveBlobCoroutine(-1,-1));
        }
        else if (actionIndex == 2){
            setAnimation(2);
            yield return StartCoroutine(moveBlobCoroutine(-1,1));
        }
        else if (actionIndex == 3){
            setAnimation(3);
            yield return StartCoroutine(moveBlobCoroutine(1,-1));
        }
    }
    private IEnumerator moveBlobCoroutine( int xDirection, int yDirection){
        float xChange = 1*xDirection;
        float yChange = 0.58f*yDirection;
        float newPosX = pos.position.x + xChange;
        float newPosY = pos.position.y + yChange;
        while (math.abs(pos.position.x - newPosX) > 0.1 || math.abs(pos.position.y - newPosY) > 0.1 ){
            pos.position += new Vector3(xDirection * xPosChange * speed/11 , yDirection*yPosChange * speed/11, 0);
            yield return null;
        }
        pos.position = new Vector2(newPosX, newPosY);
    }
    private bool setAnimation(int actionIndex){
        Tile previousTile = currentTile;
        if (actionIndex == 0){
            if (currentTile.yIndex + 1 >= world[currentTile.xIndex].Length){
                return false;
            }
            setAnimationBool("isWalkingBack");
            currentTile = world[currentTile.xIndex][currentTile.yIndex + 1];
        }
        else if (actionIndex == 1){
            if (currentTile.yIndex - 1 < 0){
                return false;
            }
            setAnimationBool("isWalkingFront");
            currentTile = world[currentTile.xIndex][currentTile.yIndex - 1];
        }
        else if (actionIndex == 2){
            if (currentTile.xIndex - 1 < 0){
                return false;
            }
            setAnimationBool("isWalkingSide");
            GetComponent<SpriteRenderer>().flipX = false;
            currentTile = world[currentTile.xIndex - 1][currentTile.yIndex];
        }
        else if (actionIndex == 3){
            if (currentTile.xIndex + 1 >= world.Length){
                return false;
            }
            setAnimationBool("isWalkingSide");
            GetComponent<SpriteRenderer>().flipX = true;
            currentTile = world[currentTile.xIndex + 1][currentTile.yIndex];
        }
        if (currentTile is MudTile && !isSlipping){
            mudCoroutine = StartCoroutine(setSpeed(speed/2));
        }
        else if (previousTile is MudTile){
            mudCoroutine = StartCoroutine(setSpeed(originalSpeed));
        }

        return true;

    }
    private int getActionIndex( float xDif, float yDif){
        if (xDif > 0 && yDif > 0){
            return 0;
        }
        if (xDif < 0 && yDif < 0){
            return 1;
        }
        if (xDif < 0 && yDif > 0){
            return 2;
        }
        if (xDif > 0 && yDif < 0 ){
            return 3;
        }
        return -1;
    }

    private void setAnimationBool(string animationBool){
            myAnimator.SetBool(currentAnimation, false);
            myAnimator.SetBool(animationBool, true);
            currentAnimation = animationBool;
    }
    private IEnumerator setSpeed(float newSpeed){
        yield return new WaitForSeconds(0.4f);
        speed = newSpeed;
    }
    private bool checkSlip(int pointCount, float xDif, float yDif){
        if (currentTile is IceTile && UnityEngine.Random.Range(0,2) == 1){
            if (mudCoroutine != null){
                StopCoroutine(mudCoroutine);
            }
            isSlipping = true;
            int count = 0;
            int actionIndex = getActionIndex(xDif, yDif);
            lr.positionCount = pointCount + 1;
            while (currentTile is IceTile && setAnimation(actionIndex)){
                count++;
                if (count == 10){break;}
            }
            // Debug.Log(count + "pointCOunt:" + pointCount);
            Vector3Int beforeSlip = tilemap.WorldToCell(linePos[lr.positionCount - 1]);
            if (actionIndex == 0){
                for (int i=0 ; i<count; i++){
                    pointCount++;
                    Vector3Int duringSlip =new Vector3Int(beforeSlip.x + i + 1, beforeSlip.y, 0);
                    connector.setPoint(duringSlip);
                }
            }
            else if (actionIndex == 1){           
                for (int i=0 ; i<count; i++){
                    pointCount++;
                    Vector3Int duringSlip =new Vector3Int(beforeSlip.x - i - 1, beforeSlip.y, 0);
                    connector.setPoint(duringSlip);
                }
            }
            else if (actionIndex == 2){
                for (int i=0 ; i<count; i++){
                    pointCount++;
                    Vector3Int duringSlip =new Vector3Int(beforeSlip.x, beforeSlip.y + i + 1, 0);
                    connector.setPoint(duringSlip);
                }
            }
            else if (actionIndex == 3){
                for (int i=0 ; i<count; i++){
                    pointCount++;
                    Vector3Int duringSlip =new Vector3Int(beforeSlip.x, beforeSlip.y - i - 1, 0);
                    connector.setPoint(duringSlip);
                }
            }
            linePos = new Vector3[lr.positionCount];
            lr.GetPositions(linePos);
            return true;
        }
        return false;
    }
    private void showLoopWarning(){
        Debug.Log("infinite loop");
    }

    private IEnumerator followLine(){
        isMoving = true;
        linePos = new Vector3[lr.positionCount];
        lr.GetPositions(linePos);
        int pointCount = 0;
        while (true){  
            Vector3 currentPoint = linePos[pointCount];
            transform.position = Vector3.MoveTowards(transform.position, currentPoint, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, currentPoint) == 0){
                // if (isSlipping){
                //     speed = originalSpeed*3;
                // }
                
                if (pointCount == lr.positionCount - 1 || currentTile.hasApple){ break;}
                pointCount++;
                Vector3 nextPoint = linePos[pointCount];
                float xDif = nextPoint.x - currentPoint.x;
                float yDif = nextPoint.y - currentPoint.y;
                if (!isSlipping) {setAnimation(getActionIndex(xDif,yDif));}
                if (!isSlipping && checkSlip(pointCount, xDif, yDif)){
                    speed = originalSpeed*4;
                }
                // Debug.Log(pointCount + " AND!!" + lr.positionCount);
            }
            yield return null;
        }
        speed = originalSpeed;
        isSlipping = false;
        if (currentTile.hasApple){
            myAnimator.SetBool(currentAnimation, false);
            myAnimator.SetBool("isIdle", true);
            myAnimator.SetBool("isHappy", true);
            connectorObject.SetActive(false);
        }
        else{
            setAnimationBool("isIdle");
        }
        currentPoint = lr.GetPosition(pointCount);
        lr.positionCount = 1;
        lr.SetPosition(0, currentPoint);
        isMoving = false;
    }
}
