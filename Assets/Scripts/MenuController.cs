using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("ProfileButton")]
    public TextMeshProUGUI profileButtoName;

    void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager não encontrado!");
            return;
        }

        if (profileButtoName != null)
        {
            profileButtoName.text = SaveManager.Instance.GetCurrentProfileName();
        }
    }

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

    public void GetToShop()
    {
        SceneManager.LoadScene("Shop");
    }

    public void GetToLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }

    public void GetToProfiles()
    {
        SceneManager.LoadScene("ProfileSelection");
    }
}