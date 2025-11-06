using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [Header("Player Progress")]
    public RectTransform characterIcon;
    public RectTransform startPosition;
    public RectTransform endPosition;

    [Header("Save Score UI")]
    public GameObject saveScorePanel;

    [Header("UI Panels")]
    public GameObject nextPhasePanel;

    [Header("UI Elements")]
    public Slider timerSlider;
    public TextMeshProUGUI leftButtonText;
    public TextMeshProUGUI rightButtonText;
    public TextMeshProUGUI nextPhaseText;

    [Header("Dot Display System")]
    public GameObject dotPrefab;
    public Transform leftDotsContainer;
    public Transform rightDotsContainer;
    
    [Header("Animated Backgrounds")]
    public Animator backgroundAnimator;
    public List<RuntimeAnimatorController> phaseBackgroundControllers;

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
    private List<PhaseResult> sessionResults = new List<PhaseResult>();

    //private string userId;
    //private DatabaseReference reference;

    void Start()
    {
        currentPhaseIndex = 0;
        if (phaseList == null || phaseList.Count == 0)
        {
            Debug.LogError("ERROR: Phase List is not configured in the Inspector.");
            this.enabled = false;
            return;
        }
        SetupPhase(currentPhaseIndex);
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

    void SetupPhase(int index)
    {
        if (index >= phaseList.Count)
        {
            Debug.LogError("ERROR: Trying to access a non-existent phase index.");
            this.enabled = false;
            return;
        }
        currentPhaseConfig = phaseList[index];

        if (backgroundAnimator != null && phaseBackgroundControllers != null && index < phaseBackgroundControllers.Count)
        {
            backgroundAnimator.runtimeAnimatorController = phaseBackgroundControllers[index];
        }

        score = 0;
        step = 0;
        currentTime = 0f;
        isGamePaused = false;
        
        if (timerSlider != null) timerSlider.value = 0f;
        if (nextPhasePanel != null) nextPhasePanel.SetActive(false);
        
        UpdateDisplay();
    }

    void Update()
    {
        if (!this.enabled || isGamePaused) return;

        if (currentTime < currentPhaseConfig.totalTime)
        {
            currentTime += Time.deltaTime;
            currentTime = Mathf.Clamp(currentTime, 0, currentPhaseConfig.totalTime);

            if (timerSlider != null)
            {
                timerSlider.value = currentTime / currentPhaseConfig.totalTime;
            }
        }
        else
        {
            step = currentPhaseConfig.maxSteps;
            CheckForPhaseCompletion();
        }

        UpdateCharacterPosition();
    }

    void UpdateDisplay()
    {
        leftNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        rightNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        while (rightNumber == leftNumber)
        {
            rightNumber = Random.Range(currentPhaseConfig.minNumber, currentPhaseConfig.maxNumber + 1);
        }

        switch (currentPhaseConfig.displayMode)
        {
            case DisplayMode.Numbers:
                SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                break;
            
            case DisplayMode.Dots:
                leftNumber = UpdateDots(leftDotsContainer, leftNumber, leftButtonText);
                rightNumber = UpdateDots(rightDotsContainer, rightNumber, rightButtonText);
                break;
            
            case DisplayMode.Mixed:
                if (Random.Range(0, 2) == 0)
                {
                    SetupNumberDisplay(leftButtonText, leftDotsContainer, leftNumber);
                    rightNumber = UpdateDots(rightDotsContainer, rightNumber, rightButtonText);
                }
                else
                {
                    leftNumber = UpdateDots(leftDotsContainer, leftNumber, leftButtonText);
                    SetupNumberDisplay(rightButtonText, rightDotsContainer, rightNumber);
                }
                break;
        }
        
        if (leftNumber == rightNumber)
        {
            Debug.LogWarning("Visual values became equal after dot placement adjustment. Rerolling turn.");
            UpdateDisplay();
        }
    }

    void SetupNumberDisplay(TextMeshProUGUI textElement, Transform dotsContainer, int number)
    {
        if(textElement != null)
        {
            textElement.gameObject.SetActive(true);
            textElement.text = number.ToString();
        }
        if(dotsContainer != null) 
        {
            dotsContainer.gameObject.SetActive(false);
        }
    }
    
    int UpdateDots(Transform container, int amount, TextMeshProUGUI textElement)
    {
        if(textElement != null) textElement.gameObject.SetActive(false);
        if(container != null) container.gameObject.SetActive(true);
        
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
                Debug.LogWarning("Could not place dot #" + (i + 1) + ". Final amount will be adjusted.");
                break;
            }
        }
        return placedDots.Count;
    }

    void CheckForPhaseCompletion()
    {
        if (step >= currentPhaseConfig.maxSteps)
        {
            var result = new PhaseResult
            {
                phaseIndex = currentPhaseIndex,
                phaseName = currentPhaseConfig.phaseName,
                score = this.score,
                timeTaken = this.currentTime
            };
            sessionResults.Add(result);

            Debug.Log(sessionResults);

            currentPhaseIndex++;
            if (currentPhaseIndex < phaseList.Count)
            {
                isGamePaused = true;
                if(nextPhaseText != null) nextPhaseText.text = "Phase '" + currentPhaseConfig.phaseName + "' Complete!";
                if(nextPhasePanel != null) nextPhasePanel.SetActive(true);
            }
            else
            {
                isGamePaused = true;
                Debug.Log("Game finished");
                if (saveScorePanel != null) saveScorePanel.SetActive(true);
            }
        }
        else
        {
            UpdateDisplay();
        }
    }

    void UpdateCharacterPosition()
    {
        if (characterIcon == null) return;

        float progressByTime = currentTime / currentPhaseConfig.totalTime;
        float progressBySteps = (float)step / currentPhaseConfig.maxSteps;

        float finalProgress = Mathf.Max(progressByTime, progressBySteps);

        finalProgress = Mathf.Clamp01(finalProgress);

        characterIcon.anchoredPosition = Vector2.Lerp(startPosition.anchoredPosition, endPosition.anchoredPosition, finalProgress);
    }

    public void GoToNextPhase()
    {
        SetupPhase(currentPhaseIndex);
    }
    
    public void GoToGameModes()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameModes");
    }

    public void OnLeftButtonClick()
    {
        if (isGamePaused) return;
        if (leftNumber > rightNumber)
        {
            score++;
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
        }
        step++;
        CheckForPhaseCompletion();
    }
}