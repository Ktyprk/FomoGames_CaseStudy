using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Elements")]
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI currentLevelText;
    
    [Header("Win Panel")]
    public GameObject winPanel;
    public Button nextLevelButton;

    [Header("Lose Panel")]
    public GameObject losePanel;
    public Button retryButton;

    private void Awake()
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

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        HideWinPanel();
        HideLosePanel();
    }

    public void UpdateMoveCount(int currentMoves, int maxMoves)
    {
        if (moveCountText != null)
        {
            if (maxMoves > 0)
            {
                int remainingMoves = maxMoves - currentMoves;
                moveCountText.text = $"Moves: {remainingMoves}";
                moveCountText.gameObject.SetActive(true);
            }
            else
            {
                moveCountText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateLevelNumber(int levelNumber)
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = $"Level {levelNumber}";
        }
    }

    public void ShowWinPanel(int usedMoves, int maxMoves)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    public void HideLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }
    
    void OnNextLevelButtonClicked()
    {
        HideWinPanel();
        
        if (LevelController.Instance != null)
        {
            LevelController.Instance.LoadNextLevelFromButton();
        }
    }

    void OnRetryButtonClicked()
    {
        HideLosePanel();
        
        if (LevelController.Instance != null)
        {
            LevelController.Instance.RestartLevel();
        }
    }

    private void OnDestroy()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(OnNextLevelButtonClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryButtonClicked);
        }
    }
}