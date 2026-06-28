using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance {  get; private set; }

    [Header("UIóvëfÇÃéQè∆")]
    [SerializeField] private TextMeshProUGUI playerStatusText;
    [SerializeField] private TextMeshProUGUI logText;

    private List<string> actionLogs = new List<string>();
    [SerializeField] private int maxLogCount = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdatePlayerStatus(int currentHp,int maxHp,int currentMp,int maxMp) 
    {
        if (playerStatusText == null) return;

        playerStatusText.text = $"HP : {currentHp} / {maxHp}\nMP : {currentMp} / {maxMp}";
    }

    public void AddLog() 
    {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
