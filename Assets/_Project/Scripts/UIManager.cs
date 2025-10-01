using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI moveCountText;
    public Button resetButton;
    public GameManager gameManager;

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => gameManager.ResetLevel());
        }
    }
}