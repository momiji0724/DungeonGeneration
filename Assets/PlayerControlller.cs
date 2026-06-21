using UnityEngine;

public class PlayerControlller : MonoBehaviour
{
    private BspTopDown mapGenerator;
    private Vector2Int gridPosition;

    public void SetupPlayer(BspTopDown generator,Vector2Int startGridPos) 
    {
        mapGenerator = generator;
        gridPosition = startGridPos;
        UpdateWorldPosition();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int movement = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) movement.y = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) movement.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) movement.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) movement.x = 1;

        if (movement != Vector2Int.zero) 
        {
            TryMove(movement);
        }


    }

    void TryMove(Vector2Int direction) 
    {
        Vector2Int targetGridPos = gridPosition + direction ;
        int[,] grid = mapGenerator.GetMapGrid();

        if(targetGridPos.x >= 0 && targetGridPos.x < grid.GetLength(0) 
           && targetGridPos.y >= 0 && targetGridPos.y < grid.GetLength(1)) 
        {
            if (grid[targetGridPos.x,targetGridPos.y] == 1) 
            {
                gridPosition = targetGridPos;
                UpdateWorldPosition();
            }
        }
    }

    void UpdateWorldPosition() 
    {
        int[,]grid = mapGenerator.GetMapGrid();
        int size = grid.GetLength(0);

        float worldX = gridPosition.x - size / 2;
        float worldY = gridPosition.y - size / 2;

        transform.position = new Vector3(worldX + 0.5f, worldY + 0.5f,0);
    }
}
