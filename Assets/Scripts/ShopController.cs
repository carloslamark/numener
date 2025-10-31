using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public SkinDatabase skinDatabase;
    public GameObject shopItemPrefab; // Prefab de um item da loja (Botão com Imagem)
    public Transform shopItemParent; // O container da lista de skins
    public Image currentEquippedSkinDisplay; // Opcional: uma imagem que mostra a skin atual

    void Start()
    {
        PopulateShop();
        UpdateEquippedDisplay();
    }

    public void PopulateShop()
    {
        foreach (Transform child in shopItemParent) { Destroy(child.gameObject); }

        PlayerData data = SaveManager.Instance.CurrentPlayerData;

        foreach (SkinData skin in skinDatabase.allSkins)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, shopItemParent);

            // Encontra os componentes no prefab
            Image itemImage = itemGO.transform.Find("SkinImage").GetComponent<Image>();
            Button itemButton = itemGO.GetComponent<Button>();
            TextMeshProUGUI itemText = itemGO.GetComponentInChildren<TextMeshProUGUI>();

            if (data.unlockedSkinIDs.Contains(skin.skinID))
            {
                // DESBLOQUEADA
                itemImage.sprite = skin.shopIcon;
                itemImage.color = Color.white; // Cor normal
                itemText.text = skin.skinName;

                // Adiciona o listener para equipar
                itemButton.onClick.AddListener(() => OnEquipClicked(skin));

                // Marca se está equipada
                if (data.equippedSkinID == skin.skinID)
                {
                    itemButton.interactable = false; // Já está equipada
                    itemText.text = skin.skinName + " (Equipado)";
                }
            }
            else
            {
                // BLOQUEADA
                itemImage.sprite = skin.shopIcon; // Mostra o sprite, mas escuro
                itemImage.color = Color.black; // Cor de "bloqueado"
                itemText.text = "???";
                itemButton.interactable = false; // Não pode clicar
            }
        }
    }

    void OnEquipClicked(SkinData skin)
    {
        SaveManager.Instance.EquipSkin(skin.skinID);
        PopulateShop(); // Atualiza a loja para mostrar o novo item equipado
        UpdateEquippedDisplay();
    }

    void UpdateEquippedDisplay()
    {
        // 1. Verifica se temos um local para mostrar a imagem e se o SaveManager está pronto.
        if (currentEquippedSkinDisplay == null || SaveManager.Instance == null || SaveManager.Instance.CurrentPlayerData == null)
        {
            return; // Sai da função se algo estiver faltando
        }

        // 2. Pega o ID da skin que o jogador equipou (que está salvo no PlayerData)
        string equippedID = SaveManager.Instance.CurrentPlayerData.equippedSkinID;

        if (string.IsNullOrEmpty(equippedID))
        {
            Debug.LogWarning("Nenhuma skin equipada encontrada no save.");
            return;
        }

        // 3. Procura no seu SkinDatabase pela skin que tem esse ID
        SkinData equippedSkinData = null;
        foreach (SkinData skin in SaveManager.Instance.skinDatabase.allSkins)
        {
            if (skin.skinID == equippedID)
            {
                equippedSkinData = skin;
                break; // Achamos!
            }
        }

        // 4. Se encontrou a skin, atualiza a imagem!
        if (equippedSkinData != null)
        {
            currentEquippedSkinDisplay.sprite = equippedSkinData.shopIcon;
        }
        else
        {
            Debug.LogWarning("Não foi possível encontrar o SkinData para o ID: " + equippedID);
        }
    }
    public void GoToMainMenu()
    {
        // Certifique-se de que o nome da sua cena de menu é "MainMenu"
        SceneManager.LoadScene("MainMenu");
    }
}