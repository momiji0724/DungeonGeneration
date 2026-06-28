using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    private BspTopDown mapGenerator;
    private Vector2Int gridPosition;
    private bool isShaking = false;

    [Header("長押し移動の設定")]
    [SerializeField] private float moveInterval = 0.15f;
    private float moveTimer = 0f;

    public CharacterStatus status;

    public event Action OnTurnConsumed;

    public void SetupPlayer(BspTopDown generator,Vector2Int startGridPos) 
    {
        mapGenerator = generator;
        gridPosition = startGridPos;
        UpdateWorldPosition();
    }

    private void Awake()
    {
        status = GetComponent<CharacterStatus>();
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (status != null)
        {
            status.isPlayer = true;
            status.maxHp = 30;
            status.currentHp = 30;
            status.attackPower = 2;

            status.currentMp = status.maxMp;

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdatePlayerStatus
                (
                    status.currentHp,
                    status.maxHp,
                    status.currentMp,
                    status.maxMp
                );
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (mapGenerator == null || isShaking) return;
        if(moveTimer > 0f) 
        {
            moveTimer -= Time.deltaTime;
        }

        Vector2Int movement = Vector2Int.zero;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) movement.y = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) movement.y = -1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) movement.x = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) movement.x = 1;

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (status != null) status.HealHp(1);
            OnTurnConsumed?.Invoke();
            
        }

        if (movement != Vector2Int.zero && moveTimer <= 0f) 
        {
            bool isShiftPressd = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (isShiftPressd)
            {
                TryDash(movement);
                moveTimer = moveInterval * 2f;
            }
            else 
            {
                TryMove(movement);
                moveTimer = moveInterval;
            }
                
        }
        

    }

    void TryMove(Vector2Int direction) 
    {
        if (mapGenerator == null) return;

        Vector2Int targetGridPos = gridPosition + direction;

        if (!mapGenerator.IsWalkable(targetGridPos))
        {
            Debug.Log("Playerが壁に衝突");
            if (CameraShaker.Instance != null) CameraShaker.Instance.Shake(0.08f, 0.12f);
            StartCoroutine(ShakeRoutine(direction));
            return;
        };

        EnemyController enemy = mapGenerator.GetEnemyAtPosition(targetGridPos);
        if (enemy != null) 
        {
            Debug.Log("PlayerがEnemyに衝突");
            if (CameraShaker.Instance != null) CameraShaker.Instance.Shake(0.08f, 0.12f);
            StartCoroutine(ShakeRoutine(direction));

            // バトル処理
            CharacterStatus enemyStatus = enemy.GetComponent<CharacterStatus>();
            if (enemyStatus != null && status != null) 
            {
                enemyStatus.TakeDamage(status.attackPower);
            }
            if (enemyStatus.currentHp > 0) 
            {
                enemy.AttackPlayer(status);
            }


            OnTurnConsumed?.Invoke();
            return;
        }

        
            gridPosition = targetGridPos;
            UpdateWorldPosition();
            
            if(status != null) 
            {
            status.HealHp(1);
            }
            
            OnTurnConsumed?.Invoke();
        
    }

    void TryDash(Vector2Int direction) 
    {
        if (mapGenerator == null) return;
        Vector2Int currentCheckPos = gridPosition;
        int stepsMoved = 0;

        while (true) 
        {
            Vector2Int nextPos = currentCheckPos + direction;

            if(!mapGenerator.IsWalkable(nextPos)||mapGenerator.GetEnemyAtPosition(nextPos) != null) 
            {
                break;
            }

            currentCheckPos = nextPos;
            stepsMoved++;
        }

        if (stepsMoved == 0) 
        {
            TryMove(direction);
            return;
        }

        gridPosition = currentCheckPos;
        UpdateWorldPosition();

        if(status != null) 
        {
            status.HealHp(stepsMoved);
        }

        for(int i = 0; i< stepsMoved; i++) 
        {
            OnTurnConsumed?.Invoke();
        }
        Debug.Log($"Shift移動 : {stepsMoved} マスダッシュしました");
    }
    
    private IEnumerator ShakeRoutine(Vector2Int direction) 
    {
        isShaking = true;

        Vector3 basePosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
        Vector3 shakeOffset = -new Vector3(direction.x, direction.y, 0) * 0.2f;

        float elapsed = 0f;
        float duration = 0.04f;
        while (elapsed < duration) 
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(basePosition, basePosition + shakeOffset, elapsed / duration);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration) 
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(basePosition, basePosition, elapsed / duration);
            yield return null;

        }
        transform.position = basePosition;
        isShaking = false;
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
