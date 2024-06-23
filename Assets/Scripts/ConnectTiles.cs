using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
public class ConnectTiles : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap

    private LineRenderer lineRenderer;
    private Vector3 startAnchor;

    [SerializeField] float xOffset;
    [SerializeField] float yOffset;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        tilemap = FindObjectOfType<Tilemap>();
        lineRenderer.positionCount = 0;
    
        BoundsInt bounds = tilemap.cellBounds;
        int centerX = bounds.xMax;
        int centerY = bounds.yMax;
        startAnchor = tilemap.CellToWorld(new Vector3Int (centerX, centerY, 0));
        // Debug.Log($"{startAnchor.x} and {startAnchor.y}");
        startAnchor.y -= 0.25f;
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(0, startAnchor);

    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)){
            onLeftClick();
        }
    }
    private Vector2 findMiddlePoint(Vector3Int tilePos){
        // Adjust by the tileAnchor to find the center
        Vector3 cellWorldPosition = tilemap.CellToWorld(tilePos);
        cellWorldPosition.x += xOffset;
        cellWorldPosition.y += yOffset;
        Vector3 middlePoint = cellWorldPosition;

        return middlePoint + tilemap.tileAnchor;
    }
    private void onLeftClick() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3Int cellMousePos = tilemap.WorldToCell(mousePos);
        setPoint(cellMousePos);
    }

    public void setPoint(Vector3Int cellPos){
        // Debug.Log($"{mousePos.x} and {mousePos.y} and {mousePos.z}");
        // Debug.Log($"{cellPos.x} and {cellPos.y}");

        Vector3 prevLineVector = tilemap.WorldToCell(lineRenderer.GetPosition(lineRenderer.positionCount - 1));
        bool isValidMove = math.abs(cellPos.x - prevLineVector.x) + math.abs(cellPos.y - prevLineVector.y) == 1;
        // Debug.Log($"{cellMousePos.x} and {prevLineVector.x} and {cellMousePos.z}and {prevLineVector.y}");
        if (tilemap.HasTile(cellPos) && isValidMove) {
            lineRenderer.positionCount++;
            Vector3 pointAnchor = findMiddlePoint(cellPos);
            lineRenderer.SetPosition(lineRenderer.positionCount-1, pointAnchor);
        }
    }
}
