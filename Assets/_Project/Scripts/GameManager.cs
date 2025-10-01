using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject blockPrefab;
    public GameObject exitPrefab;

    [Header("Settings")]
    public float cellSize = 1f;
    public TextureManager textureManager;
    
    [Header("Level Data")]
    public TextAsset levelJsonFile;

    [Header("Materials")]
    public Material blockMaterial; 
    public Material exitMaterial;

    [Header("Exit Colors")]
    public Color[] exitColors = new Color[]
    {
        new Color(0.23f, 0.51f, 0.96f), // 0: Blue
        Color.white,                      // 1: White
        new Color(0.06f, 0.73f, 0.51f), // 2: Green
        new Color(0.96f, 0.62f, 0.04f), // 3: Orange
        new Color(0.94f, 0.29f, 0.29f)  // 4: Red
    };

    private LevelData currentLevel;
    private List<Block> blocks = new List<Block>();
    private int moveCount = 0;
    private int blocksRemaining;

    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private List<ExitTrigger> exitTriggers = new List<ExitTrigger>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadLevel();
        CreateLevel();
    }

    void LoadLevel()
    {
        if (levelJsonFile != null)
        {
            currentLevel = JsonUtility.FromJson<LevelData>(levelJsonFile.text);
            blocksRemaining = currentLevel.MovableInfo.Count;
        }
        else
        {
            Debug.LogError("Level JSON file not assigned!");
        }
    }

    void CreateLevel()
    {
        foreach (var cellInfo in currentLevel.CellInfo)
        {
            Vector3 position = GetWorldPosition(cellInfo.Row, cellInfo.Col);
            GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
            cell.name = $"Cell_{cellInfo.Row}_{cellInfo.Col}";
            cellObjects[new Vector2Int(cellInfo.Row, cellInfo.Col)] = cell;
        }
        
        foreach (var exitInfo in currentLevel.ExitInfo)
        {
            Vector3 cellPosition = GetWorldPosition(exitInfo.Row, exitInfo.Col);
            Vector3 exitOffset = GetExitOffset(exitInfo.Direction);
            GameObject exit = Instantiate(exitPrefab, cellPosition + exitOffset, Quaternion.identity);
            
            ExitTrigger exitComponent = exit.GetComponent<ExitTrigger>();
            if (exitComponent != null)
            {
                Color exitColor = exitColors[exitInfo.Colors];
                exitComponent.Initialize(exitInfo, exitColor, exitMaterial);
                exitTriggers.Add(exitComponent);
            }
        }
        
        foreach (var movableInfo in currentLevel.MovableInfo)
        {
            Vector3 position = GetWorldPosition(movableInfo.Row, movableInfo.Col);
            GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);
            
            Block block = blockObj.GetComponent<Block>();
            if (block != null)
            {
                block.Initialize(movableInfo, this);
                blocks.Add(block);
            }
        }
    }

    public Vector3 GetWorldPosition(int row, int col)
    {
        float x = col * cellSize;
        float z = -row * cellSize;
        return new Vector3(x, 0.25f, z);
    }

    Vector3 GetExitOffset(int direction)
    {
        float offset = cellSize * 0.5f;
        switch (direction)
        {
            case 0: return new Vector3(0, 0, offset);  // Up
            case 1: return new Vector3(offset, 0, 0);  // Right
            case 2: return new Vector3(0, 0, -offset); // Down
            case 3: return new Vector3(-offset, 0, 0); // Left
            default: return Vector3.zero;
        }
    }

    public bool IsValidPosition(int row, int col)
    {
        return cellObjects.ContainsKey(new Vector2Int(row, col));
    }

    public bool IsBlockAt(Vector2Int pos, Block excludeBlock)
    {
        return blocks.Any(b => b != null && b != excludeBlock && b.Row == pos.x && b.Col == pos.y);
    }

    public void OnBlockReachedExit(Block block)
    {
        blocks.Remove(block);
        blocksRemaining--;

        Destroy(block.gameObject);

        if (blocksRemaining <= 0)
        {
            Debug.Log("Level Complete!");
            Invoke("LoadNextLevel", 1f);
        }
    }

    void LoadNextLevel()
    {
        Debug.Log("Loading next level...");
    }

    public void IncrementMoveCount()
    {
        moveCount++;
    }

    public void ResetLevel()
    {
        foreach (var cell in cellObjects.Values)
            Destroy(cell);
        cellObjects.Clear();

        foreach (var block in blocks)
            if (block != null) Destroy(block.gameObject);
        blocks.Clear();

        foreach (var exit in exitTriggers)
            if (exit != null) Destroy(exit.gameObject);
        exitTriggers.Clear();

        moveCount = 0;
        CreateLevel();
    }
}