using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Importante para ordenar
using UnityEngine.SceneManagement;

public class LeaderboardUI : MonoBehaviour
{
    // Crie esta pequena classe ou struct dentro do seu script
    private class PlayerHighScoreEntry
    {
        public string playerName;
        public int highScore;

        public PlayerHighScoreEntry(string name, int score)
        {
            playerName = name;
            highScore = score;
        }
    }

    [Header("UI References")]
    public Transform scoreListContainer; // Arraste o 'ScoreListContainer' para cá
    public GameObject scoreEntryPrefab; // Arraste o 'ScoreEntryPrefab' para cá

    void Start()
    {
        PopulateLeaderboard();
    }

    void PopulateLeaderboard()
    {
        // 1. Limpa entradas antigas

        foreach (Transform child in scoreListContainer)
        {
            Destroy(child.gameObject);
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager não encontrado!");
            return;
        }

        // 2. Pega todos os perfis
        List<string> profileNames = SaveManager.Instance.ListAllProfileNames();

        // 3. Pega o High Score de CADA perfil
        List<PlayerHighScoreEntry> allHighScores = new List<PlayerHighScoreEntry>();
        foreach (string profileName in profileNames)
        {
            int highScore = SaveManager.Instance.GetInfinityHighScore(profileName);
            // Só adiciona ao ranking se o jogador tiver pontuado
            if (highScore > 0)
            {
                allHighScores.Add(new PlayerHighScoreEntry(profileName, highScore));
            }
        }

        // 4. Ordena a lista do maior score para o menor
        List<PlayerHighScoreEntry> sortedScores = allHighScores.OrderByDescending(entry => entry.highScore).ToList();

        // 5. Cria os itens na UI
        for (int i = 0; i < sortedScores.Count; i++)
        {
            GameObject entryGO = Instantiate(scoreEntryPrefab, scoreListContainer);
            PlayerHighScoreEntry entryData = sortedScores[i];

            // Encontra os textos no prefab
            TextMeshProUGUI rankText = entryGO.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entryGO.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryGO.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();

            // Preenche os dados
            rankText.text = (i + 1).ToString() + ".";
            nameText.text = entryData.playerName;
            scoreText.text = entryData.highScore.ToString();
        }
    }

    // Função para o seu botão "Voltar"
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}