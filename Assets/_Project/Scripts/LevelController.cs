using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    private int currentLevelIndex = 0;
    private int currentMoveCount = 0;
    private int maxMoves = 0;
    private bool levelCompleted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    void LoadLevel(int levelIndex)
    {
        string levelPath = $"Levels/Level{levelIndex + 1}";
        TextAsset levelJson = Resources.Load<TextAsset>(levelPath);

        if (levelJson != null)
        {
            Debug.Log($"Level {levelIndex + 1} yükleniyor...");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevelFromJson(levelJson);
                
                int levelMaxMoves = GameManager.Instance.GetMaxMoves();
                SetMaxMoves(levelMaxMoves);
            }
        }
        else
        {
            Debug.LogError($"Level {levelIndex + 1} bulunamadı! Resources/Levels/Level{levelIndex + 1}.json dosyasını kontrol edin.");
        }
    }

    public void SetMaxMoves(int moves)
    {
        maxMoves = moves;
        currentMoveCount = 0;
        levelCompleted = false;
        UpdateUI();
    }

    public void IncrementMoveCount()
    {
        if (levelCompleted) return;

        currentMoveCount++;
        UpdateUI();

        if (maxMoves > 0 && currentMoveCount >= maxMoves)
        {
            Invoke("ResetCurrentLevel", 1f);
        }
    }

    public void OnLevelComplete()
    {
        if (levelCompleted) return;
        levelCompleted = true;
        Invoke("LoadNextLevel", 1.5f);
    }

    void LoadNextLevel()
    {
        currentLevelIndex++;
        
        string levelPath = $"Levels/Level{currentLevelIndex + 1}";
        TextAsset levelJson = Resources.Load<TextAsset>(levelPath);

        if (levelJson != null)
        {
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("Tüm leveller tamamlandı! İlk level'a dönülüyor...");
            currentLevelIndex = 0;
            LoadLevel(currentLevelIndex);
        }
    }

    void ResetCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    void UpdateUI()
    {
        if (maxMoves > 0)
        {
            Debug.Log($"Hamle: {currentMoveCount}/{maxMoves}");
        }
        else
        {
            //Debug.Log($"Hamle: {currentMoveCount} (Sınırsız)");
        }
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public int GetCurrentMoveCount()
    {
        return currentMoveCount;
    }

    public int GetMaxMoves()
    {
        return maxMoves;
    }

    public int GetRemainingMoves()
    {
        if (maxMoves == 0)
            return -1; 
            
        return Mathf.Max(0, maxMoves - currentMoveCount);
    }
    }