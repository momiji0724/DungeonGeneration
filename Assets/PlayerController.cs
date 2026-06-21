using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private BspTopDown mapGenerator;
    private Vector2Int gridPosition;

    public event Action OnTurnConsumed;

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
        if (mapGenerator == null) return;
        Vector2Int movement = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) movement.y = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) movement.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) movement.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) movement.x = 1;

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            OnTurnConsumed?.Invoke();
        }


        if (movement != Vector2Int.zero) 
        {
            TryMove(movement);
        }


    }

    void TryMove(Vector2Int direction) 
    {
        if (mapGenerator == null) return;

        Vector2Int targetGridPos = gridPosition + direction;

        if (mapGenerator.IsWalkable(targetGridPos))
        {
            gridPosition = targetGridPos;
            UpdateWorldPosition();

            OnTurnConsumed?.Invoke();
        }
    }
    

    void UpdateWorldPosition() 
    {
        if (mapGenerator == null) return;

        transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f,0);
    }
    public Vector2Int GetGridPosition() 
    {
        return gridPosition;
    }
}
