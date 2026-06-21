using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private BspTopDown mapGenerator;
    private Vector2Int gridPosition;
    private RectInt currentRoom;
    private bool hasRoom = false;

    public void SetupEnemy(BspTopDown generator, Vector2Int startGridPos,RectInt room,bool isInRoom)
    {
        Debug.Log($"SetupEnemy : {startGridPos}");

        mapGenerator = generator;
        gridPosition = startGridPos;
        currentRoom = room;
        hasRoom = isInRoom;
        UpdateWorldPosition();
    }

    public void TakeTurn(Vector2Int playerGridPos) 
    {
        if (mapGenerator == null) return;

        if (hasRoom && IsPlayerInSameRoom(playerGridPos))
        {
            Debug.Log("Player‚ªEnemy‚Ì•”‰®‚ÉN“üI");
            MoveTowards(playerGridPos);
        }
    }

    private bool IsPlayerInSameRoom(Vector2Int playerPos) 
    {
        return playerPos.x >= currentRoom.xMin && playerPos.x < currentRoom.xMax &&
        playerPos.y >= currentRoom.yMin && playerPos.y < currentRoom.yMax;
    }

    private void MoveTowards(Vector2Int targetPos) 
    {
        Vector2Int direction = Vector2Int.zero;

        if (Mathf.Abs(targetPos.x - gridPosition.x) > Mathf.Abs(targetPos.y - gridPosition.y)) 
        {
            direction.x = targetPos.x > gridPosition.x ? 1 : -1;
        }
        else 
        {
            direction.y = targetPos.y > gridPosition.y ? 1 : -1;
        }

        TryMove(direction);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void TryMove(Vector2Int direction)
    {
        if (mapGenerator == null) return;

        Vector2Int targetGridPos = gridPosition + direction;

        if (mapGenerator.IsWalkable(targetGridPos)) 
        {
            gridPosition = targetGridPos;
            UpdateWorldPosition();
        }

    }

    void UpdateWorldPosition()
    {
        Debug.Log($"Move Enemy To : {gridPosition}");
        transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
    }
}
