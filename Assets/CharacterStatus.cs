using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [Header("基本ステータス")]
    [SerializeField] public int maxHp = 20;
    public int currentHp;
    [SerializeField] public int maxMp = 10;
    public int currentMp;
    [SerializeField] public int attackPower = 1;

    [Header("識別用")]
    [SerializeField] public bool isPlayer = false;

    private void Awake() 
    {
        currentHp = maxHp;
        currentMp = maxMp;
    }
    void Start()
    {

    }


    public void TakeDamage(int damage) 
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        string unitName = isPlayer ? "Player" : gameObject.name;
        Debug.Log($"{unitName}は{damage}のダメージを受けた！(残りHP : {currentHp} / {maxHp})");

        UpdateUI();

        if(currentHp <= 0) 
        {
            Die();
        }
    }

    public void HealHp(int amount) 
    {
        if (currentHp <= 0) return;
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        UpdateUI();
    }

    private void UpdateUI() 
    {
        if(isPlayer && GameUIManager.Instance != null) 
        {
            GameUIManager.Instance.UpdatePlayerStatus(currentHp, maxHp, currentMp, maxMp);

        }
    }

    private void Die() 
    {
        string unitName = isPlayer ? "Player" : gameObject.name;
        Debug.Log($"{unitName}は倒れた！");
        if (isPlayer) 
        {
            //ゲームオーバー演出
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        
    }
}
