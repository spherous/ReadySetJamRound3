using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class EndGamePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private Button selectedButton = null;
    private void Awake() {
        Score score = GameObject.FindObjectOfType<Score>();
        scoreText.text = $"Score: {score.current}";

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        if(score.current > highscore)
            PlayerPrefs.SetInt("Highscore", score.current);

        retryButton.Select();
        selectedButton = retryButton;
    }

    public void MakeSelection(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(selectedButton == retryButton)
        {
            selectedButton = quitButton;
            quitButton.Select();
        }
        else if(selectedButton == quitButton || selectedButton == null)
        {
            selectedButton = retryButton;
            retryButton.Select();
        }
    }
}