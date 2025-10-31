using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Globalization;


[System.Serializable]
public class DifficultyConfig
{
    public string difficultyName;
    public DisplayMode displayMode;
    public int minNumber = 1;
    public int maxNumber = 9;
    [Header("Dot Customization")]
    public float minDotSize = 0.4f;
    public float maxDotSize = 0.8f;
}

public class InfinityModeController : MonoBehaviour
{
    [Header("Game Configuration")]
    public float startingTime = 120f;
    public float timeGainedOnCorrect = 2f;
    public float timeLostOnIncorrect = 5f;
    public int scoreToCompleteLevel = 52;

    [Header("Feedback Visuals")]
    public float feedbackDelay = 0.75f;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    [Header("Player Progress")]
    public RectTransform characterIcon;
    public RectTransform startPosition;
    public RectTransform endPosition;

    [Header("UI Elements")]
    public Slider timerSlider;
    public TextMeshProUGUI leftButtonText;
    public TextMeshProUGUI rightButtonText;
    public TextMeshProUGUI difficultyText;
    public GameObject gameOverPanel;

    [Header("Dot Display System")]
    public GameObject dotPrefab;
    public Transform leftDotsContainer;
    public Transform rightDotsContainer;

    [Header("Difficulty Levels (5 total)")]
    public List<DifficultyConfig> difficultyLevels;

    [Header("Animated Backgrounds")]
    public Animator backgroundAnimator;
    public List<RuntimeAnimatorController> backgroundAnimControllers;

    [Header("Q-Learning Parameters")]
    [Range(0, 1)] public float learningRate = 0.1f;
    [Range(0, 1)] public float discountFactor = 0.9f;
    [Range(0, 1)] public float explorationRate = 0.1f;

    private float[,] qTable;
    private const int PERFORMANCE_STATES = 3;
    private const int TOTAL_STATES = 5 * PERFORMANCE_STATES;
    private const int TOTAL_ACTIONS = 3;
    private int previousState;
    private int previousAction;
    private float rewardSumForWindow;
    private int currentDifficultyIndex = 2;
    private const int PERFORMANCE_WINDOW = 5;

    private float currentTime;
    private int leftNumber;
    private int rightNumber;
    private int score;
    private int step;
    private bool isGamePaused = false;

    private Color defaultButtonColor;

    void Start()
    {
        if (difficultyLevels == null || difficultyLevels.Count != 5)
        {
            Debug.LogError("ERROR: Configure exactly 5 difficulty levels in the Inspector.");
            this.enabled = false;
            return;
        }

        qTable = new float[TOTAL_STATES, TOTAL_ACTIONS];
        LoadQTable();

        currentTime = startingTime;
        score = 0;
        step = 0;
        isGamePaused = false;

        previousState = GetCurrentState();
        previousAction = GetBestAction(previousState);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateDisplay();
        UpdateBackground();
        UpdateDifficultyText();

        if (SaveManager.Instance != null && characterIcon != null)
        {
            // Pega o Animator do ícone do personagem
            Animator charAnimator = characterIcon.GetComponent<Animator>();
            if (charAnimator != null)
            {
                // Pede ao SaveManager o controller da skin equipada
                RuntimeAnimatorController equippedController = SaveManager.Instance.GetEquippedSkinController();
                if (equippedController != null)
                {
                    // Atribui o novo "cérebro" de animação
                    charAnimator.runtimeAnimatorController = equippedController;
                }
                else
                {
                    Debug.LogWarning("Nenhum Animator Controller de skin foi encontrado!");
                }
            }
            else
            {
                Debug.LogError("O objeto CharacterIcon não tem um componente Animator!");
            }
        }
    }

    void OnDestroy()
    {
        SaveQTable();
    }

    void Update()
    {
        if (isGamePaused) return;

        currentTime -= Time.deltaTime;

        if (timerSlider != null)
        {
            timerSlider.value = currentTime / startingTime;
        }

        if (currentTime <= 0)
        {
            currentTime = 0;
            isGamePaused = true;
            Debug.Log("Time's up! Game Over. Final Score: " + score);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AddInfinityModeScore(score);
            }
            else
            {
                Debug.LogError("ERRO: SaveManager não encontrado para salvar o score do Modo Infinito!");
            }
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        UpdateCharacterPosition();
    }

    // --- FUNÇÕES DE CARREGAMENTO E SALVAMENTO ---
    void LoadQTable()
    {
        string savedTable = PlayerPrefs.GetString("QTable_InfinityMode", "");

        if (string.IsNullOrEmpty(savedTable))
        {
            Debug.Log("No PlayerPrefs found. Loading pre-trained Q-Table from Resources...");
            TextAsset qTableAsset = Resources.Load<TextAsset>("q_table_infinity");
            if (qTableAsset != null)
            {
                savedTable = qTableAsset.text;
            }
            else
            {
                Debug.LogWarning("No pre-trained Q-Table found in Resources/q_table_infinity.txt. Initializing a new one.");
                return;
            }
        }

        string[] rows = savedTable.Trim().Split('\n');
        for (int i = 0; i < TOTAL_STATES; i++)
        {
            if (i < rows.Length)
            {
                string[] values = rows[i].Trim().Split(',');
                for (int j = 0; j < TOTAL_ACTIONS; j++)
                {
                    if (j < values.Length && !string.IsNullOrEmpty(values[j]))
                    {
                        qTable[i, j] = float.Parse(values[j], CultureInfo.InvariantCulture);
                    }
                }
            }
        }
        Debug.Log("Q-Table loaded successfully.");
    }

    void SaveQTable()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < TOTAL_STATES; i++)
        {
            for (int j = 0; j < TOTAL_ACTIONS; j++)
            {
                sb.Append(qTable[i, j].ToString(CultureInfo.InvariantCulture)).Append(',');
            }
            if (sb.Length > 0) sb.Length--;
            sb.Append('\n');
        }

        PlayerPrefs.SetString("QTable_InfinityMode", sb.ToString());
        PlayerPrefs.Save();
        Debug.Log("Q-Table saved to PlayerPrefs.");
    }


    void UpdateCharacterPosition()
    {
        if (characterIcon == null || startPosition == null || endPosition == null) return;

        float scoreInCurrentCycle = score % scoreToCompleteLevel;
        float progress = scoreInCurrentCycle / (float)scoreToCompleteLevel;

        progress = Mathf.Clamp01(progress);

        characterIcon.anchoredPosition = Vector2.Lerp(startPosition.anchoredPosition, endPosition.anchoredPosition, progress);
    }

    private IEnumerator ShowFeedbackSequence(bool wasCorrect)
    {
        isGamePaused = true;

        Image correctButtonImage;
        Image incorrectButtonImage;

        if (leftNumber > rightNumber)
        {
            correctButtonImage = leftButtonText.GetComponentInParent<Image>();
            incorrectButtonImage = rightButtonText.GetComponentInParent<Image>();
        }
        else
        {
            correctButtonImage = rightButtonText.GetComponentInParent<Image>();
            incorrectButtonImage = leftButtonText.GetComponentInParent<Image>();
        }

        if (defaultButtonColor == default)
        {
            defaultButtonColor = correctButtonImage.color;
        }

        correctButtonImage.color = correctColor;
        incorrectButtonImage.color = incorrectColor;

        if (wasCorrect) OnCorrectAnswer();
        else OnIncorrectAnswer();

        yield return new WaitForSeconds(feedbackDelay);

        correctButtonImage.color = defaultButtonColor;
        incorrectButtonImage.color = defaultButtonColor;

        FinishTurn();

        isGamePaused = false;
    }


    void FinishTurn()
    {
        step++;

        if (step % PERFORMANCE_WINDOW == 0 && step > 0)
        {
            AdjustDifficultyQLearning();
        }

        if (step > 0 && step % 52 == 0)
        {
            UpdateBackground();
        }

        UpdateDisplay();
    }

    public void OnLeftButtonClick()
    {
        if (isGamePaused) return;

        bool wasCorrect = leftNumber > rightNumber;
        StartCoroutine(ShowFeedbackSequence(wasCorrect));
    }

    public void OnRightButtonClick()
    {
        if (isGamePaused) return;

        bool wasCorrect = rightNumber > leftNumber;
        StartCoroutine(ShowFeedbackSequence(wasCorrect));
    }

    int GetCurrentState()
    {
        int performanceState;
        int correctAnswersInWindow = (int)((rewardSumForWindow + PERFORMANCE_WINDOW) / 2);
        if (correctAnswersInWindow <= 1) performanceState = 0;
        else if (correctAnswersInWindow <= 3) performanceState = 1;
        else performanceState = 2;
        return (currentDifficultyIndex * PERFORMANCE_STATES) + performanceState;
    }



    void OnCorrectAnswer()
    {
        int oldScore = score;
        score++;

        currentTime += timeGainedOnCorrect;
        if (currentTime > startingTime) currentTime = startingTime;
        rewardSumForWindow += 1f;

        if ((oldScore / scoreToCompleteLevel) < (score / scoreToCompleteLevel))
        {
            UpdateBackground();

            if (characterIcon != null) characterIcon.anchoredPosition = startPosition.anchoredPosition;

            SaveManager.Instance.UnlockRandomSkin();
            Debug.Log("LEVEL UP! Changing background and resetting character.");
        }
    }

    void OnIncorrectAnswer()
    {
        currentTime -= timeLostOnIncorrect;
        rewardSumForWindow -= 1f;
    }

    void AdjustDifficultyQLearning()
    {
        int currentState = GetCurrentState();
        float maxFutureQ = GetMaxQValue(currentState);
        float oldQValue = qTable[previousState, previousAction];
        float newQValue = oldQValue + learningRate * (rewardSumForWindow + discountFactor * maxFutureQ - oldQValue);
        qTable[previousState, previousAction] = newQValue;

        int actionToTake;
        if (Random.value < explorationRate)
        {
            actionToTake = Random.Range(0, TOTAL_ACTIONS);
        }
        else
        {
            actionToTake = GetBestAction(currentState);
        }
        ApplyAction(actionToTake);

        previousState = currentState;
        previousAction = actionToTake;
        rewardSumForWindow = 0;
    }

    float GetMaxQValue(int state)
    {
        float maxQ = float.MinValue;
        for (int i = 0; i < TOTAL_ACTIONS; i++)
        {
            if (qTable[state, i] > maxQ) maxQ = qTable[state, i];
        }
        return maxQ;
    }

    int GetBestAction(int state)
    {
        float maxQ = float.MinValue;
        int bestAction = 1;
        for (int i = 0; i < TOTAL_ACTIONS; i++)
        {
            if (qTable[state, i] > maxQ)
            {
                maxQ = qTable[state, i];
                bestAction = i;
            }
        }
        return bestAction;
    }

    void ApplyAction(int action)
    {
        if (action == 0 && currentDifficultyIndex > 0)
        {
            currentDifficultyIndex--;
        }
        else if (action == 2 && currentDifficultyIndex < difficultyLevels.Count - 1)
        {
            currentDifficultyIndex++;
        }
        UpdateDifficultyText();
    }

    void UpdateDisplay()
    {
        DifficultyConfig currentConfig = difficultyLevels[currentDifficultyIndex];

        leftNumber = Random.Range(currentConfig.minNumber, currentConfig.maxNumber + 1);
        rightNumber = Random.Range(currentConfig.minNumber, currentConfig.maxNumber + 1);
        while (rightNumber == leftNumber)
        {
            rightNumber = Random.Range(currentConfig.minNumber, currentConfig.maxNumber + 1);
        }

        switch (currentConfig.displayMode)
        {
            case DisplayMode.Numbers:
                SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                break;

            case DisplayMode.Dots:
                leftNumber = UpdateDots(leftDotsContainer, leftNumber);
                rightNumber = UpdateDots(rightDotsContainer, rightNumber);
                break;

            case DisplayMode.Mixed:
                if (Random.Range(0, 2) == 0)
                {
                    SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                    rightNumber = UpdateDots(rightDotsContainer, rightNumber);
                }
                else
                {
                    leftNumber = UpdateDots(leftDotsContainer, leftNumber);
                    SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                }
                break;
        }

        if (leftNumber == rightNumber)
        {
            Debug.LogWarning("Visual values became equal. Rerolling turn.");
            UpdateDisplay();
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

    int UpdateDots(Transform container, int amount)
    {
        foreach (Transform child in container) { Destroy(child.gameObject); }
        if (container == null) return 0;

        // Ativa o container e desativa os textos correspondentes
        if (container == leftDotsContainer) leftButtonText.gameObject.SetActive(false);
        if (container == rightDotsContainer) rightButtonText.gameObject.SetActive(false);
        container.gameObject.SetActive(true);

        DifficultyConfig currentConfig = difficultyLevels[currentDifficultyIndex];
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
                float randomScale = Random.Range(currentConfig.minDotSize, currentConfig.maxDotSize);
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
                Debug.LogWarning("Could not place dot #" + (i + 1) + ". Final amount will be adjusted.");
                break;
            }
        }
        return placedDots.Count;
    }

    void UpdateDifficultyText()
    {
        if (difficultyText != null)
        {
            difficultyText.text = "Dificuldade: " + difficultyLevels[currentDifficultyIndex].difficultyName;
        }
    }

    public void GoToGameModes()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameModes");
    }
    void UpdateBackground()
    {
        if (backgroundAnimator == null || backgroundAnimControllers == null || backgroundAnimControllers.Count == 0) return;

        int backgroundIndex = (score / scoreToCompleteLevel) % backgroundAnimControllers.Count;

        Debug.Log("Tentando trocar para o background no índice " + backgroundIndex + ", que é o controller: " + backgroundAnimControllers[backgroundIndex].name);

        backgroundAnimator.runtimeAnimatorController = backgroundAnimControllers[backgroundIndex];
    }
}