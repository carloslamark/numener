using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameModes");
    }

    public void QuitGame()
    {
        Debug.Log("SAINDO DO JOGO...");

        Application.Quit();
    }

    public void GetToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void StartTutorialMode()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void StartStoryMode()
    {
        SceneManager.LoadScene("History");
    }

    public void StartInfinityMode()
    {
        SceneManager.LoadScene("Infinity");
    }
}