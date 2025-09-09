using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum DisplayMode { Numbers, Dots, Mixed }

[System.Serializable]
public class PhaseConfig
{
    public string phaseName;
    public string description;
    public DisplayMode displayMode;
    public float totalTime = 120f;
    public int maxSteps = 52;
    public int minNumber = 1;
    public int maxNumber = 9;
    [Header("Dot Customization")]
    public float minDotSize = 0.8f;
    public float maxDotSize = 1.5f;
}

public class StoryModeController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject nextPhasePanel;
    public GameObject gameOverPanel;

    [Header("UI Elements")]
    public Slider timerSlider;
    public TextMeshProUGUI leftButtonText;
    public TextMeshProUGUI rightButtonText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nextPhaseText;

    [Header("Dot Display System")]
    public GameObject dotPrefab;
    public Transform leftDotsContainer;
    public Transform rightDotsContainer;

    [Header("Phase Configuration")]
    public List<PhaseConfig> phaseList;

    private int currentPhaseIndex;
    private PhaseConfig currentPhaseConfig;

    private float currentTime;
    private int leftNumber;
    private int rightNumber;
    private int score;
    private int step;
    private bool isGamePaused = false;

    void Start()
    {
        currentPhaseIndex = 0;
        if (phaseList == null || phaseList.Count == 0) { return; }
        SetupPhase(currentPhaseIndex);
    }

    void SetupPhase(int index)
    {
        if (index >= phaseList.Count) { return; }
        currentPhaseConfig = phaseList[index];
        score = 0;
        step = 0;
        currentTime = 0f;
        isGamePaused = false;
        if (timerSlider != null) timerSlider.value = 0f;
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (nextPhasePanel != null) nextPhasePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreText();
        UpdateDisplay();
    }

    void Update()
    {
        if (!this.enabled || isGamePaused) return;
        if (currentTime >= currentPhaseConfig.totalTime)
        {
            Debug.Log("Time's up! Game Over.");
            isGamePaused = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            return;
        }
        if (timerSlider != null)
        {
            currentTime += Time.deltaTime;
            timerSlider.value = currentTime / currentPhaseConfig.totalTime;
        }
    }

    void UpdateDisplay()
    {
        leftNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        rightNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        while (rightNumber == leftNumber)
        {
            rightNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        }

        // Switch para decidir os modos
        switch (currentPhaseConfig.displayMode)
        {
            case DisplayMode.Numbers:
                SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                break;

            case DisplayMode.Dots:
                SetupDotDisplay(leftButtonText, leftDotsContainer, leftNumber);
                SetupDotDisplay(rightButtonText, rightDotsContainer, rightNumber);
                break;

            case DisplayMode.Mixed:
                if (Random.Range(0, 2) == 0)
                {
                    SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                    SetupDotDisplay(rightButtonText, rightDotsContainer, rightNumber);
                }
                else
                {
                    SetupDotDisplay(leftButtonText, leftDotsContainer, leftNumber);
                    SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                }
                break;
        }
    }

    void SetupNumberDisplay(TextMeshProUGUI textElement, Transform dotsContainer, int number)
    {
        if (textElement != null)
        {
            textElement.gameObject.SetActive(true);
            textElement.text = number.ToString();
        }
        if (dotsContainer != null)
        {
            dotsContainer.gameObject.SetActive(false);
        }
    }

    void SetupDotDisplay(TextMeshProUGUI textElement, Transform dotsContainer, int number)
    {
        if (textElement != null)
        {
            textElement.gameObject.SetActive(false);
        }
        if (dotsContainer != null)
        {
            dotsContainer.gameObject.SetActive(true);
            UpdateDots(dotsContainer, number);
        }
    }

    int UpdateDots(Transform container, int amount)
    {
        foreach (Transform child in container) { Destroy(child.gameObject); }

        if (container == null) return 0;

        RectTransform containerRect = container.GetComponent<RectTransform>();
        float halfWidth = containerRect.rect.width / 2;
        float halfHeight = containerRect.rect.height / 2;

        List<RectTransform> placedDots = new List<RectTransform>();
        const int maxPlacementAttempts = 100;

        for (int i = 0; i < amount; i++)
        {
            bool positionFound = false;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                GameObject newDotObject = Instantiate(dotPrefab, container);
                RectTransform dotRect = newDotObject.GetComponent<RectTransform>();

                float randomScale = Random.Range(currentPhaseConfig.minDotSize, currentPhaseConfig.maxDotSize);
                dotRect.localScale = new Vector3(randomScale, randomScale, 1f);
                float dotRadius = (dotRect.rect.width / 2) * randomScale;

                float randomX = Random.Range(-halfWidth + dotRadius, halfWidth - dotRadius);
                float randomY = Random.Range(-halfHeight + dotRadius, halfHeight - dotRadius);
                dotRect.anchoredPosition = new Vector2(randomX, randomY);

                bool isOverlapping = false;
                foreach (RectTransform placedDot in placedDots)
                {
                    float placedDotRadius = (placedDot.rect.width / 2) * placedDot.localScale.x;
                    float distance = Vector2.Distance(dotRect.anchoredPosition, placedDot.anchoredPosition);
                    if (distance < dotRadius + placedDotRadius)
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                if (!isOverlapping)
                {
                    placedDots.Add(dotRect);
                    positionFound = true;
                    break;
                }
                else
                {
                    Destroy(newDotObject);
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning("Não foi possível encontrar uma posição para a bolinha " + (i + 1) + ". O número final será ajustado.");
                break;
            }
        }

        return placedDots.Count;
    }

    void CheckForPhaseCompletion()
    {
        // No modo infinito, utilizar essa função para colocar o Q-Learning
        // A dificuldade decide o parâmetro a ser passado para o UpdateDisplay
        // Ele modifica os modos
        if (step >= currentPhaseConfig.maxSteps)
        {
            currentPhaseIndex++;
            if (currentPhaseIndex < phaseList.Count)
            {
                isGamePaused = true;
                nextPhaseText.text = "Phase '" + currentPhaseConfig.phaseName + "' Complete!";
                nextPhasePanel.SetActive(true);
            }
            else
            {
                isGamePaused = true;
                Debug.Log("YOU WON THE GAME!");
                if (victoryPanel != null) victoryPanel.SetActive(true);
            }
        }
        else
        {
            UpdateDisplay();
        }
    }

    public void GoToNextPhase() { SetupPhase(currentPhaseIndex); }
    public void GoToMainMenu() { UnityEngine.SceneManagement.SceneManager.LoadScene("GameModes"); }

    public void OnLeftButtonClick()
    {
        if (isGamePaused) return;
        if (leftNumber > rightNumber)
        {
            score++;
            UpdateScoreText();
        }
        step++;
        CheckForPhaseCompletion();
    }

    public void OnRightButtonClick()
    {
        if (isGamePaused) return;
        if (rightNumber > leftNumber)
        {
            score++;
            UpdateScoreText();
        }
        step++;
        CheckForPhaseCompletion();
    }

    void UpdateScoreText() { scoreText.text = "Score: " + score; }
}