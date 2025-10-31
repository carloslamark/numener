using System.Collections;
using System.Collections.Generic; // Para List<>
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    public TMP_InputField newProfileInput;
    public Button playButton;
    public Transform profileButtonParent; // Arraste o ProfileListContainer aqui
    public GameObject profileButtonPrefab; // Arraste o prefab do botão aqui

    void Start()
    {
        if (playButton != null) playButton.interactable = false; // Começa desabilitado
        ListExistingProfiles();
    }

    void ListExistingProfiles()
    {
        // Limpa botões antigos
        foreach (Transform child in profileButtonParent) { Destroy(child.gameObject); }

        List<string> profileNames = SaveManager.Instance.ListAllProfileNames();

        foreach (string profileName in profileNames)
        {
            GameObject buttonGO = Instantiate(profileButtonPrefab, profileButtonParent);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = profileName;
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(() => SelectProfile(profileName));

            // --- ADMIN FEATURE: Botão Deletar (opcional) ---
            // Encontra um botão filho chamado "DeleteButton" (você precisa adicionar isso ao prefab)
            Button deleteButton = buttonGO.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (deleteButton != null)
            {
                //deleteButton.gameObject.SetActive(isAdminMode); // Controla visibilidade
                deleteButton.onClick.AddListener(() => DeleteProfile(profileName));
            }
        }
    }

    public void OnCreateProfileClick()
    {
        string newName = newProfileInput.text;
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogError("Nome do perfil não pode ser vazio.");
            return;
        }

        // Em vez de chamar CreateNewProfile diretamente, iniciamos a corrotina
        StartCoroutine(CreateAndLoadProfile(newName));
    }

    private IEnumerator CreateAndLoadProfile(string newName)
    {
        if (SaveManager.Instance.CreateNewProfile(newName))
        {
            yield return null;

            SelectProfile(newName);
            ListExistingProfiles();
            newProfileInput.text = "";
        }
        else
        {
            Debug.LogError("Falha ao criar perfil (nome vazio ou já existe?)");
        }
    }

    void SelectProfile(string profileName)
    {
        if (SaveManager.Instance.LoadProfile(profileName))
        {
            Debug.Log($"Perfil '{profileName}' selecionado.");
            if (playButton != null) playButton.interactable = true;
            // Opcional: Destacar o botão selecionado
        }
        else
        {
            Debug.LogError($"Falha ao carregar perfil '{profileName}'.");
            if (playButton != null) playButton.interactable = false;
        }
    }
    void DeleteProfile(string profileName)
    {
        // Adicionar confirmação aqui ("Tem certeza?")
        if (SaveManager.Instance.DeleteProfile(profileName))
        {
            ListExistingProfiles(); // Atualiza a lista
            if (playButton != null) playButton.interactable = false; // Desabilita Jogar se o perfil ativo foi deletado
        }
    }


    public void OnPlayClick()
    {
        if (!string.IsNullOrEmpty(SaveManager.Instance.CurrentProfileName))
        {
            // Mude "MainMenu" para o nome da sua cena de menu principal
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("Nenhum perfil selecionado para jogar.");
        }
    }
}