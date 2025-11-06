using UnityEngine;
using UnityEngine.UI; // <<--- MUITO IMPORTANTE! Certifique-se que esta linha está presente
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using NUnit.Framework;
// using UnityEngine.UIElements; // <- Você não precisa desta linha, pode apagar

public class TutorialController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image tutorialDisplayImage; // O "quadro" único que vai mostrar as imagens
    public Button nextButton;
    public Button backButton;

    [Header("Tutorial Pages")]
    public List<Sprite> tutorialPages; // <<--- AQUI ESTÁ A MUDANÇA: Uma lista de Sprites (imagens)

    private int currentPageIndex;

    void Start()
    {
        // Verifica se temos páginas para mostrar
        if (tutorialPages == null || tutorialPages.Count == 0)
        {
            Debug.LogError("Nenhuma página de tutorial foi adicionada à lista 'tutorialPages' no Inspector!");
            return;
        }

        // Começa na primeira página
        currentPageIndex = 0;
        if (tutorialDisplayImage != null)
        {
            UpdateTutorialUI();
        }
    }

    public void NextImage()
    {
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            currentPageIndex++;
            UpdateTutorialUI();
        }
    }

    public void BackImage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateTutorialUI();
        }
    }

    // A VERSÃO CORRIGIDA
    private void UpdateTutorialUI()
    {
        // 1. Define o sprite (a imagem) do nosso "quadro"
        tutorialDisplayImage.sprite = tutorialPages[currentPageIndex];

        // 2. Mostra ou esconde os botões de navegação
        if (backButton != null)
        {
            // Mostra o botão "Voltar" APENAS se não estivermos na primeira página
            backButton.gameObject.SetActive(currentPageIndex > 0);
        }

        if (nextButton != null)
        {
            // Mostra o botão "Avançar" APENAS se não estivermos na última página
            nextButton.gameObject.SetActive(currentPageIndex < tutorialPages.Count - 1);
        }
    }

    public void GoToGameModes()
    {
        SceneManager.LoadScene("GameModes");
    }
}