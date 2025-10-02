using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    private int currentLevelIndex = 0;
    private int currentMoveCount = 0;
    private int maxMoves = 0;
    private bool levelCompleted = false;

    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string HIGHEST_LEVEL_KEY = "HighestLevel";

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
        ResetProgress();
        LoadSavedLevel();
    }
    
    void LoadSavedLevel()
    {
        currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
        Debug.Log($"Kaydedilmiş seviye yükleniyor: Level {currentLevelIndex + 1}");
        
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
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateLevelNumber(levelIndex + 1);
                }
            }
            
            SaveCurrentLevel(levelIndex);
        }
        else
        {
            Debug.LogError($"Level {levelIndex + 1} bulunamadı! Resources/Levels/Level{levelIndex + 1}.json dosyasını kontrol edin.");
        }
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
        Debug.Log("Level resetleniyor...");
        LoadLevel(currentLevelIndex);
    }

    void SaveCurrentLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, levelIndex);
        PlayerPrefs.Save();
        Debug.Log($"Level {levelIndex + 1} kaydedildi.");
    }

    void SaveHighestLevel(int levelIndex)
    {
        int highestLevel = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 0);
        
        if (levelIndex > highestLevel)
        {
            PlayerPrefs.SetInt(HIGHEST_LEVEL_KEY, levelIndex);
            PlayerPrefs.Save();
            Debug.Log($"En yüksek seviye güncellendi: Level {levelIndex + 1}");
        }
    }

    public int GetHighestLevel()
    {
        return PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 0);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);
        PlayerPrefs.DeleteKey(HIGHEST_LEVEL_KEY);
        PlayerPrefs.Save();
        
        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex);
        
        Debug.Log("Oyun ilerlemesi sıfırlandı!");
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
            Debug.Log("Hamle sayısı doldu!");
            OnOutOfMoves();
        }
    }

    void OnOutOfMoves()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLosePanel();
        }
    }

    public void OnLevelComplete()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        Debug.Log($"Level {currentLevelIndex + 1} tamamlandı! Kullanılan hamle: {currentMoveCount}/{maxMoves}");
        
        SaveHighestLevel(currentLevelIndex);
        
        Invoke("ShowWinPanel", 0.5f);
    }

    void ShowWinPanel()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinPanel(currentMoveCount, maxMoves);
        }
    }

    public void LoadNextLevelFromButton()
    {
        LoadNextLevel();
    }

    public void RestartLevel()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideWinPanel();
            UIManager.Instance.HideLosePanel();
        }
        
        ResetCurrentLevel();
    }

    void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(currentMoveCount, maxMoves);
        }

        if (maxMoves > 0)
        {
            Debug.Log($"Hamle: {currentMoveCount}/{maxMoves}");
        }
    }
}