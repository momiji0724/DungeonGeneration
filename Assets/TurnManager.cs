using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private PlayerController player;
    private List<EnemyController> enemies = new List<EnemyController>();
    private int turnCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(Initialize), 0.2f);
    }
    void Initialize()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.GetComponent<PlayerController>();

            BspTopDown mapGenerator = FindFirstObjectByType<BspTopDown>();
            if (mapGenerator != null) 
            {
                enemies = mapGenerator.GetEnemies();
            }

            if (player != null)
            {
                player.OnTurnConsumed += AdvanceTrun;
                Debug.Log("ターンカウント:開始");
                return;
            }
        }
        Debug.LogError("初期化に失敗しました");
        
    }

    void MoveEnemy() 
    {
        if (player == null) return;

        Vector2Int playerPos = player.GetGridPosition();
        foreach(EnemyController enemy in enemies) 
        {
            if(enemy != null) 
            {
                enemy.TakeTurn(playerPos);
            }
        }
    }

    void AdvanceTrun() 
    {
        turnCount++;
        Debug.Log($"ターンカウント:{turnCount}ターン");
        MoveEnemy();
    }

    private void OnDestroy()
    {
        if (player != null) 
        {
            player.OnTurnConsumed -= AdvanceTrun;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
