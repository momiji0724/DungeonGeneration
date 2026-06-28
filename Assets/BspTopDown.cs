using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.U2D;
using static BspTopDown;

public class BspTopDown : MonoBehaviour
{
    [Header("Playerの設定")]
    public GameObject playerPrefab;

    [Header("Enemyの設定")]
    public GameObject enemyPrefab;

    private System.Collections.Generic.List<EnemyController> spawnedEnemies = new System.Collections.Generic.List<EnemyController>();
    public System.Collections.Generic.List<EnemyController> GetEnemies() => spawnedEnemies;


    public int minRegionSize = 10;
    public int maxDepth = 4;

    [Header("タイルマップの設定")]
    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    private int mapSize = 60;
    private int[,] mapGrid;

    private List<RectInt> allRooms = new List<RectInt>();
    private PlayerController spawnedPlayer;
    public class Region
    {
        public int x, y, width, height;
        public Region leftChild;
        public Region rightChild;
        public RectInt room;

        public Region(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.leftChild = null;
            this.rightChild = null;
        }

        public bool isLeaf() => leftChild == null && rightChild == null;
    }

    public void SplitRegion(Region currentRegion,int currentDepth) 
    {
        if (currentDepth >= maxDepth) return;
        if (currentRegion.width < minRegionSize * 2 || currentRegion.height < minRegionSize * 2) return;

        bool splitHorizontally = Random.value > 0.5f;

        if(currentRegion.width > currentRegion.height && (float)currentRegion.width / currentRegion.height >= 1.25f) 
        {
            splitHorizontally = false;
        }
        else if (currentRegion.height > currentRegion.width && (float)currentRegion.height / currentRegion.width >= 1.25f) 
        {
            splitHorizontally = true;
        }

        if (splitHorizontally) 
        {
            int splitPoint = Random.Range(minRegionSize, currentRegion.height - minRegionSize);

            currentRegion.leftChild = new Region(currentRegion.x, currentRegion.y, currentRegion.width,splitPoint);
            currentRegion.rightChild = new Region(currentRegion.x, currentRegion.y + splitPoint, currentRegion.width, currentRegion.height -splitPoint);
            Vector3 lineStart = new Vector3(currentRegion.x, currentRegion.y + splitPoint, 0);
            Vector3 lineEnd = new Vector3(currentRegion.x + currentRegion.width, currentRegion.y + splitPoint, 0);
            Debug.DrawLine(lineStart, lineEnd, Color.red, 100f); // 100秒間表示
        }
        else
        {
            int splitPoint = Random.Range(minRegionSize, currentRegion.width - minRegionSize);

            currentRegion.leftChild = new Region(currentRegion.x, currentRegion.y, splitPoint, currentRegion.height);
            currentRegion.rightChild = new Region(currentRegion.x + splitPoint, currentRegion.y, currentRegion.width - splitPoint, currentRegion.height);


            // 縦割りの線をSceneビューに引く（緑色で表示）
            Vector3 lineStart = new Vector3(currentRegion.x + splitPoint, currentRegion.y, 0);
            Vector3 lineEnd = new Vector3(currentRegion.x + splitPoint, currentRegion.y + currentRegion.height, 0);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 100f);

        }

        SplitRegion(currentRegion.leftChild, currentDepth + 1);
        SplitRegion(currentRegion.rightChild, currentDepth + 1);
    }

    public void CreateRooms(Region region) 
    {
        if (region == null) return;

        if (region.isLeaf()) 
        {
            int roomWidth = Random.Range(minRegionSize / 2, region.width - 2);
            int roomHeight = Random.Range(minRegionSize / 2,region.height - 2);

            int roomX = region.x + Random.Range(1, region.width - roomWidth - 1);
            int roomY = region.y + Random.Range(1, region.height - roomHeight - 1);

            region.room = new RectInt(roomX, roomY, roomWidth, roomHeight);

            allRooms.Add(region.room);

            for (int x = roomX; x < roomX + roomWidth; x++) 
            {
                for(int y = roomY; y < roomY + roomHeight; y++) 
                {
                    SetGridValue(x, y, 1);
                }
            }



            Vector3 bottomLeft = new Vector3(roomX, roomY, 0);
            Vector3 topLeft = new Vector3(roomX, roomY + roomHeight, 0);
            Vector3 topRight = new Vector3(roomX + roomWidth, roomY + roomHeight, 0);
            Vector3 bottomRight = new Vector3(roomX + roomWidth, roomY, 0);

            Debug.DrawLine(bottomLeft, topLeft, Color.blue, 100f);
            Debug.DrawLine(topLeft, topRight, Color.blue, 100f);
            Debug.DrawLine(topRight, bottomRight, Color.blue, 100f);
            Debug.DrawLine(bottomRight, bottomLeft, Color.blue, 100f);

        }
        else 
        {
            CreateRooms(region.leftChild);
            CreateRooms(region.rightChild);
        }
    }

    public void ConnectRooms(Region region)
    {
        if (region == null || region.isLeaf()) return;

        ConnectRooms(region.leftChild);
        ConnectRooms(region.rightChild);

        RectInt roomA = GetClosestRoom(region.leftChild, region);
        RectInt roomB = GetClosestRoom(region.rightChild, region);


        bool wasSplitHorizontally = (region.leftChild.y + region.leftChild.height) < (region.y + region.height);

        if (wasSplitHorizontally)
        {
            int borderY = region.leftChild.y + region.leftChild.height;

            int startX = (int)roomA.center.x;
            int endX = (int)roomB.center.x;

            DigTunnel(startX, roomA.yMax, startX, borderY);
            DigTunnel(startX, borderY, endX, borderY);
            DigTunnel(endX, borderY, endX, roomB.yMin);

            Vector3 p1 = new Vector3(startX, roomA.yMax, 0);
            Vector3 p2 = new Vector3(startX, borderY, 0);
            Vector3 p3 = new Vector3(endX, borderY, 0);
            Vector3 p4 = new Vector3(endX, roomB.yMin, 0);

            Debug.DrawLine(p1, p2, Color.magenta, 100f);
            Debug.DrawLine(p2, p3, Color.magenta, 100f);
            Debug.DrawLine(p3, p4, Color.magenta, 100f);

        }
        else
        {
            int borderX = region.leftChild.x + region.leftChild.width;

            int startY = (int)roomA.center.y;
            int endY = (int)roomB.center.y;

            DigTunnel(roomA.xMax, startY, borderX, startY);
            DigTunnel(borderX, startY, borderX, endY);
            DigTunnel(borderX, endY, roomB.xMin, endY);


            Vector3 p1 = new Vector3(roomA.xMax, startY, 0);
            Vector3 p2 = new Vector3(borderX,startY, 0);
            Vector3 p3 = new Vector3(borderX, endY, 0);
            Vector3 p4 = new Vector3(roomB.xMin, endY, 0);

            Debug.DrawLine(p1, p2, Color.magenta, 100f);
            Debug.DrawLine(p2, p3, Color.magenta, 100f);
            Debug.DrawLine(p3, p4, Color.magenta, 100f);
        }
    }

    private RectInt GetClosestRoom(Region current,Region parent) 
    {
        if(current.isLeaf())return current.room;

        bool wasSplitHorizontally = (parent.leftChild.y + parent.leftChild.height) < (parent.y + parent.height);

        RectInt leftRoom = GetClosestRoom(current.leftChild, parent);
        RectInt rightRoom = GetClosestRoom(current.rightChild, parent);

        if (wasSplitHorizontally)
        {
            int borderY = parent.leftChild.y + parent.leftChild.height;

            int distLeft = Mathf.Abs(leftRoom.center.y > borderY ? leftRoom.yMin - borderY : leftRoom.yMax - borderY);
            int distRight = Mathf.Abs(rightRoom.center.y > borderY ? rightRoom.yMin - borderY : rightRoom.yMax - borderY);

            return (distLeft < distRight) ? leftRoom : rightRoom;
        }
        else 
        {
            int borderX = parent.leftChild.x + parent.leftChild.width;

            int distLeft = Mathf.Abs(leftRoom.center.x > borderX ? leftRoom.xMin - borderX : leftRoom.xMax - borderX);
            int distRight = Mathf.Abs(rightRoom.center.x > borderX ? rightRoom.xMin - borderX : rightRoom.xMax - borderX);

            return(distLeft < distRight) ? leftRoom : rightRoom;

        }

    }
    
    private void DigTunnel(int x1,int y1,int x2,int y2) 
    {
        int startX = Mathf.Min(x1, x2);
        int endX = Mathf.Max(x1, x2);
        int startY = Mathf.Min(y1, y2);
        int endY = Mathf.Max(y1, y2);

        for(int x = startX; x <= endX; x++) 
        {
            for (int y = startY; y <= endY; y++)
            {
                SetGridValue(x, y, 1);
            }
            
        }
    }

    private void SetGridValue(int x, int y,int value) 
    {
        int gridX = x + mapSize / 2;
        int gridY = y + mapSize / 2;

        if(gridX >= 0 && gridX < mapSize && gridY >=0 && gridY < mapSize) 
        {
            mapGrid[gridX,gridY] = value;
        }
    }


    private void DrawTiles() 
    {
        if(tilemap == null || floorTile == null || wallTile == null) 
        {
            Debug.Log("タイルがセットされていません");
            return;
        }

        tilemap.ClearAllTiles();

        for(int x = 0; x < mapSize; x++) 
        {
            for (int y = 0; y < mapSize; y++)
            {
                int worldX = x - mapSize / 2;
                int worldY = y - mapSize / 2;
                Vector3Int tilePos = new Vector3Int(worldX, worldY, 0);
                if (mapGrid[x,y] == 1) 
                {
                    tilemap.SetTile(tilePos,floorTile);
                }
                else 
                {
                    tilemap.SetTile(tilePos,wallTile);
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapSize = 60;
        mapGrid = new int[mapSize, mapSize];

        int leftBottomX = -mapSize / 2;
        int leftBottomY = -mapSize / 2;

        minRegionSize = 10;

        Region rootRegion = new Region(leftBottomX, leftBottomY, mapSize, mapSize);
        SplitRegion(rootRegion, 0);

        CreateRooms(rootRegion);

        ConnectRooms(rootRegion);

        DrawTiles();


        Debug.Log("ダンジョン生成完了");

        if (allRooms.Count < 2)
        {
            Debug.LogError("部屋の数が足りません");
            return;
        }

        List<RectInt> shuffledRooms = new List<RectInt>(allRooms);

        for (int i = 0; i < shuffledRooms.Count; i++)
        {
            int temp = Random.Range(i, shuffledRooms.Count);
            RectInt value = shuffledRooms[temp];
            shuffledRooms[temp] = shuffledRooms[i];
            shuffledRooms[i] = value;
        }

        RectInt playerRoom = shuffledRooms[0];

        Vector2Int playerGridPos = new Vector2Int((int)playerRoom.center.x, (int)playerRoom.center.y);
        bool playerSpawned = false;

        if (playerPrefab != null) 
        {
            GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerObj.tag = "Player";
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetupPlayer(this, playerGridPos);
                spawnedPlayer = playerController;
            }
        }

        if (enemyPrefab != null)
        {
            spawnedEnemies.Clear();

            int maxEnemiesToSpawn = 5;
            int spawnedCount = 0;

            for(int i =1; i < shuffledRooms.Count; i++) 
            {
                if (spawnedCount >= maxEnemiesToSpawn) break;

                RectInt enemyRoom = shuffledRooms[i];
                Vector2Int enemyGridPos = new Vector2Int((int)enemyRoom.center.x, (int)enemyRoom.center.y);
                GameObject enemyObj = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
                enemyObj.tag = "Enemy";
                EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.SetupEnemy(this, enemyGridPos, enemyRoom, true);
                    spawnedEnemies.Add(enemyController);
                    spawnedCount++;
                }

            }


        }

    }

    public bool IsWalkable(Vector2Int gridPos)
    {
        int ix = gridPos.x + mapSize / 2;
        int iy = gridPos.y + mapSize / 2;

        if (ix >= 0 && ix < mapSize && iy >= 0 && iy < mapSize)
        {
            return mapGrid[ix, iy] == 1;
        }
        return false;
    }

    public int[,] GetMapGrid() 
    {
        return mapGrid;
    }

    public EnemyController GetEnemyAtPosition(Vector2Int gridPos) 
    {
        foreach(var enemy in spawnedEnemies) 
        {
            if(enemy != null && enemy.GetGridPosition() == gridPos) 
            {
                return enemy;
            }
        }
        return null;
    }

    public bool IsPlayerAtPosition(Vector2Int gridPos) 
    {
        if(spawnedPlayer == null)return false;
        return spawnedPlayer.GetGridPosition() == gridPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
