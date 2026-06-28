using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyController : MonoBehaviour
{
    private BspTopDown mapGenerator;
    private Vector2Int gridPosition;
    private RectInt currentRoom;
    private bool hasRoom = false;
    private bool isShaking = false;

    public CharacterStatus Status { get; private set;}

    private void Awake()
    {
        Status = GetComponent<CharacterStatus>();

        if(Status != null) 
        {
            Status.maxHp = 10;
            Status.currentHp = 10;
            Status.attackPower = 1;
        }
    }

    public void AttackPlayer(CharacterStatus playerStatus) 
    {
        if (Status == null || playerStatus == null) return;

        Debug.Log("EnemyÇ™PlayerÇ…îΩåÇ");
        playerStatus.TakeDamage(Status.attackPower);
    }

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
        if (mapGenerator == null || isShaking) return;

        if (hasRoom && IsPlayerInSameRoom(playerGridPos))
        {
            Debug.Log("PlayerÇ™EnemyÇÃïîâÆÇ…êNì¸ÅI");
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
    public Vector2Int GetGridPosition() 
    {
        return gridPosition;
    }
    void TryMove(Vector2Int direction)
    {
        if (mapGenerator == null) return;

        Vector2Int targetGridPos = gridPosition + direction;

        if (!mapGenerator.IsWalkable(targetGridPos)) 
        {
            StartCoroutine(ShakeRoutine(direction));
            return; 
        }

        if (mapGenerator.IsPlayerAtPosition(targetGridPos)) 
        {
            Debug.Log("EnemyÇ™PlayerÇ…è’ìÀ");
            StartCoroutine(ShakeRoutine(direction));
            return;
        }
        if(mapGenerator.GetEnemyAtPosition(targetGridPos) != null) 
        {
            StartCoroutine(ShakeRoutine(direction));
            return;
        }
        
        gridPosition = targetGridPos;
        UpdateWorldPosition();

    }

    private IEnumerator ShakeRoutine(Vector2Int direction)
    {
        isShaking = true;

        Vector3 basePosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
        Vector3 shakeOffset = -new Vector3(direction.x, direction.y, 0) * 0.15f;

        float elapsed = 0f;
        float duration = 0.03f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(basePosition, basePosition + shakeOffset, elapsed / duration);
            yield return null;
        }
        elapsed = 0f;
        duration = 0.05f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(basePosition, basePosition + shakeOffset, elapsed / duration);
            yield return null;

        }
        transform.position = basePosition;
        isShaking = false;
    }

    void UpdateWorldPosition()
    {
        Debug.Log($"Move Enemy To : {gridPosition}");
        transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
    }
}
