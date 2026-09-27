using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene changes and menu actions for UI buttons in the main menu.
/// Plays selection sounds when interacting with the menu.
/// </summary>
public class MenuNavigation : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        AudioManager.instance.Play("Select");
        SceneManager.LoadScene(sceneName);
    }

    public void StartGame()
    {
        AudioManager.instance.Play("Select");
        GameObject.Find("SGameManager").GetComponent<SGameManager>().LoadGame();
    }
}
