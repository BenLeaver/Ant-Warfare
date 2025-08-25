using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

/// <summary>
/// Handles the end screen UI for the multiplayer game, showing the winning team, 
/// updating colors, and handling leaving the game.
/// </summary>
public class MEndScreenUI : NetworkBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("Color Gradients")]
    [SerializeField] private TMP_ColorGradient redGradient;
    [SerializeField] private TMP_ColorGradient greenGradient;
    [SerializeField] private TMP_ColorGradient blueGradient;
    [SerializeField] private TMP_ColorGradient purpleGradient;

    [Header("Image")]
    [SerializeField] private Image imageComponent;
    [SerializeField] private Sprite redWinScreen;
    [SerializeField] private Sprite greenWinScreen;
    [SerializeField] private Sprite blueWinScreen;
    [SerializeField] private Sprite purpleWinScreen;

    /// <summary>
    /// Leave the game, disconnecting from the game.
    /// NetworkManager and ProjectSceneManager are destroyed.
    /// </summary>
    public void LeaveGame()
    {
        if(NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Destroy(GameObject.Find("NetworkManager"));
        Destroy(GameObject.Find("ProjectSceneManager"));
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Updates the winner display on the game end screen.
    /// </summary>
    public void UpdateWinner(int winningTeam, string winningUsername)
    {
        Debug.Log("UpdateWinner() called");
        if (winningTeam == 1)
        {
            winnerText.text = $"Red Team ({winningUsername}) Wins!";
            winnerText.colorGradientPreset = redGradient;
            imageComponent.overrideSprite = redWinScreen;
        }
        else if (winningTeam == 2)
        {
            winnerText.text = $"Green Team ({winningUsername}) Wins!";
            winnerText.colorGradientPreset = greenGradient;
            imageComponent.overrideSprite = greenWinScreen;
        }
        else if (winningTeam == 3)
        {
            winnerText.text = $"Blue Team ({winningUsername}) Wins!";
            winnerText.colorGradientPreset = blueGradient;
            imageComponent.overrideSprite = blueWinScreen;
        }
        else if (winningTeam == 4)
        {
            winnerText.text = $"Purple Team ({winningUsername}) Wins!";
            winnerText.colorGradientPreset = purpleGradient;
            imageComponent.overrideSprite = purpleWinScreen;
        }
        else if (winningTeam == -1)
        {
            winnerText.text = "Draw: All Colonies Died!";
        }
    }
}
