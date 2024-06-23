using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FallAnimator : MonoBehaviour
{
    public Tilemap tilemap;
    [SerializeField] float fallSpeed;
    [SerializeField] float delayBetweenTiles;
    GameObject[] blobArray;
    GameObject[] appleArray;
    GameObject[] predArray;

    float[] initialBlobPos;
    float[] initialApplePos;
    float[] initialPredPos;
    void Start()
    {   
        DontDestroyOnLoad(gameObject);

        blobArray = GameObject.FindGameObjectsWithTag("Prey");
        appleArray = GameObject.FindGameObjectsWithTag("Apple");
        predArray = GameObject.FindGameObjectsWithTag("Predator");
        initialBlobPos = new float[blobArray.Length];
        initialApplePos = new float[appleArray.Length];
        initialPredPos = new float[predArray.Length];
        setInitialPositions();
        setTransparent();
        StartCoroutine(fallAllObjects());
    }

    IEnumerator fallAllObjects()
    {
        // Get the bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Loop through all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tilePos);
                if (tile != null)
                {
                    StartCoroutine(FallTile(tilePos));
                    yield return new WaitForSeconds(delayBetweenTiles);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        dropGameObjects();
    }

    IEnumerator FallTile(Vector3Int tilePos)
    {
        Vector3 worldPos = tilemap.CellToWorld(tilePos);
        Vector3 startPos = new Vector3(worldPos.x, worldPos.y + 6, worldPos.z); // Start position above the final position

        Sprite sprite = tilemap.GetSprite(tilePos);
        Color white = Color.white;

        GameObject tileObject = new GameObject("Tile");
        tileObject.transform.position = startPos;
        tileObject.transform.localScale = new Vector2(2,2);

        SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;


        while (tileObject.transform.position.y > worldPos.y + 0.65f) //y offset
        {
            tileObject.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        tileObject.transform.position = worldPos;

        // Set the tile back in the tilemap
        tilemap.SetColor(tilePos, white);
        Destroy(tileObject);
    }

    private void setTransparent(){
        BoundsInt bounds = tilemap.cellBounds;
        Color transparent = new Color(1, 1, 1, 0);
        // Loop through all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                tilemap.SetTileFlags(tilePos, TileFlags.None);
                tilemap.SetColor(tilePos, transparent);   
            }
        }
    }
    private void dropGameObjects(){

        for (int i = 0; i<blobArray.Length;i++){
           StartCoroutine(dropObjectsCoroutine(blobArray[i], initialBlobPos[i]));
        }
        for (int i = 0; i<appleArray.Length; i++){
           StartCoroutine(dropObjectsCoroutine(appleArray[i], initialApplePos[i]));
        }
        for (int i = 0; i<predArray.Length; i++){
           StartCoroutine(dropObjectsCoroutine(predArray[i], initialPredPos[i]));
        }
    }
    private IEnumerator dropObjectsCoroutine(GameObject obj, float initialY){
        Vector3 initialPos = obj.transform.position;
        while (obj.transform.position.y > initialY){
            obj.transform.position += Vector3.down * fallSpeed *1.5f* Time.deltaTime;
            yield return null;
        }
    }
    private void setInitialPositions(){
        for (int i = 0; i<blobArray.Length;i++){
            initialBlobPos[i] = blobArray[i].transform.position.y;
            blobArray[i].transform.position = new Vector3(blobArray[i].transform.position.x, blobArray[i].transform.position.y + 6, 0);
        }
        for (int i = 0; i<appleArray.Length; i++){
            initialApplePos[i] = appleArray[i].transform.position.y;
            appleArray[i].transform.position = new Vector3(appleArray[i].transform.position.x, appleArray[i].transform.position.y + 6, 0);
        }
        for (int i = 0; i<predArray.Length; i++){
            initialPredPos[i] = predArray[i].transform.position.y;
            predArray[i].transform.position = new Vector3(predArray[i].transform.position.x, predArray[i].transform.position.y + 6, 0);
        }
    }
}
