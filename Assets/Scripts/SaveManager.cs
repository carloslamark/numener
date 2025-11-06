using UnityEngine;
using System.IO; // Para trabalhar com arquivos
using System.Collections.Generic; // Para usar List<>
using System.Linq;

public class SaveManager : MonoBehaviour
{
    [Header("Game Data")]
    public SkinDatabase skinDatabase;
    public static SaveManager Instance { get; private set; }

    private string saveFolderPath;
    public PlayerData CurrentPlayerData { get; private set; }
    public string CurrentProfileName { get; private set; }

    private const string LastProfilePrefKey = "LastLoadedProfile";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFolderPath = Path.Combine(Application.persistentDataPath, "Profiles");
        if (!Directory.Exists(saveFolderPath))
        {
            try { Directory.CreateDirectory(saveFolderPath); }
            catch (System.Exception e) { Debug.LogError($"Falha ao criar pasta de saves: {e.Message}"); }
        }

        string lastProfile = PlayerPrefs.GetString(LastProfilePrefKey, null);
        if (!string.IsNullOrEmpty(lastProfile))
        {
            LoadProfile(lastProfile);
        }
        else
        {
            CurrentPlayerData = null;
            CurrentProfileName = null;
            Debug.Log("Nenhum perfil carregado automaticamente.");
        }
    }

    private string GetSaveFilePath(string profileName)
    {
        string cleanName = SanitizeFileName(profileName);
        if (string.IsNullOrEmpty(cleanName)) return null;
        return Path.Combine(saveFolderPath, $"profile_{cleanName}.json");
    }

    private string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        foreach (char c in Path.GetInvalidFileNameChars()) { name = name.Replace(c.ToString(), ""); }
        name = name.Trim();
        return name;
    }

    public bool LoadProfile(string profileName)
    {
        string filePath = GetSaveFilePath(profileName);
        if (filePath == null) return false;

        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                CurrentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                CurrentProfileName = profileName;
                PlayerPrefs.SetString(LastProfilePrefKey, profileName);
                Debug.Log($"Perfil '{profileName}' carregado.");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Falha ao carregar perfil '{profileName}': {e.Message}");
                InitializeNewPlayerData(profileName);
                CurrentPlayerData.playerName = profileName + " (Corrompido)";
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Perfil '{profileName}' não encontrado.");
            CurrentPlayerData = null;
            CurrentProfileName = null;
            return false;
        }
    }

    public void SaveCurrentProfileData()
    {
        if (CurrentPlayerData == null || string.IsNullOrEmpty(CurrentProfileName)) return;
        string filePath = GetSaveFilePath(CurrentProfileName);

        if (filePath == null) return;

        try
        {
            string json = JsonUtility.ToJson(CurrentPlayerData, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"Dados do perfil '{CurrentProfileName}' salvos.");
        }
        catch (System.Exception e) { Debug.LogError($"Falha ao salvar perfil '{CurrentProfileName}': {e.Message}"); }
    }

    public bool CreateNewProfile(string profileName)
    {

        if (string.IsNullOrEmpty(profileName)) return false;
        string filePath = GetSaveFilePath(profileName);
        
        if (filePath == null) return false;

        if (File.Exists(filePath))
        {
            Debug.LogError($"Perfil '{profileName}' já existe!");
            return false;
        }

        InitializeNewPlayerData(profileName);
        SaveCurrentProfileData();
        PlayerPrefs.SetString(LastProfilePrefKey, profileName);
        Debug.Log($"Novo perfil '{profileName}' criado.");
        return true;
    }

    private void InitializeNewPlayerData(string profileName)
    {
        CurrentPlayerData = new PlayerData();
        CurrentPlayerData.playerName = profileName;
        CurrentPlayerData.storyModeResults = new List<PhaseResult>();
        CurrentPlayerData.infinityModeScoreHistory = new List<int>();
        CurrentProfileName = profileName;

        // --- NOVAS LINHAS AQUI ---
        CurrentPlayerData.unlockedSkinIDs = new List<string>();

        // Desbloqueia todas as skins marcadas como "padrão"
        foreach (var skin in skinDatabase.allSkins)
        {
            if (skin.isUnlockedByDefault)
            {
                CurrentPlayerData.unlockedSkinIDs.Add(skin.skinID);
                // Define a primeira skin padrão como equipada
                if (string.IsNullOrEmpty(CurrentPlayerData.equippedSkinID))
                {
                    CurrentPlayerData.equippedSkinID = skin.skinID;
                }
            }
        }
    }

    public void UnloadProfile()
    {
        CurrentPlayerData = null;
        CurrentProfileName = null;
        PlayerPrefs.DeleteKey(LastProfilePrefKey);
        Debug.Log("Perfil descarregado.");
    }

    public bool DeleteProfile(string profileName)
    {
        string filePath = GetSaveFilePath(profileName);
        if (filePath == null) return false;

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Perfil '{profileName}' deletado.");
                if (CurrentProfileName == profileName) UnloadProfile();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Falha ao deletar perfil '{profileName}': {e.Message}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Perfil '{profileName}' não encontrado para deletar.");
            return false;
        }
    }

    // --- Funções Específicas para Atualizar os Dados do Perfil Ativo ---
    public void UpdateStoryModeResults(List<PhaseResult> results)
    {
        if (CurrentPlayerData == null) return;
        CurrentPlayerData.storyModeResults = results;
        SaveCurrentProfileData();
    }

    public void AddInfinityModeScore(int newScore)
    {
        if (CurrentPlayerData == null)
        {
            Debug.LogError("Nenhum perfil carregado para adicionar score do Modo Infinito.");
            string lastProfile = PlayerPrefs.GetString(LastProfilePrefKey, null);
            if (!string.IsNullOrEmpty(lastProfile)) LoadProfile(lastProfile);
            else InitializeNewPlayerData("JogadorPadrão");
        }

        if (CurrentPlayerData.infinityModeScoreHistory == null)
        {
            CurrentPlayerData.infinityModeScoreHistory = new List<int>();
        }

        CurrentPlayerData.infinityModeScoreHistory.Add(newScore);

        SaveCurrentProfileData();

        Debug.Log($"Score {newScore} do Modo Infinito adicionado ao histórico do perfil '{CurrentProfileName}'.");
    }

    public List<string> ListAllProfileNames()
    {
        List<string> profileNames = new List<string>();
        if (!Directory.Exists(saveFolderPath)) return profileNames;
        string[] profileFiles = Directory.GetFiles(saveFolderPath, "profile_*.json");
        foreach (string filePath in profileFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string profileName = fileName.Substring("profile_".Length);
            profileNames.Add(profileName);
        }
        return profileNames;
    }

    public void UnlockRandomSkin()
    {
        if (CurrentPlayerData == null || skinDatabase == null) return;

        // 1. Cria uma lista de skins que o jogador AINDA NÃO TEM
        List<SkinData> lockedSkins = new List<SkinData>();
        foreach (var skin in skinDatabase.allSkins)
        {
            if (!CurrentPlayerData.unlockedSkinIDs.Contains(skin.skinID))
            {
                lockedSkins.Add(skin);
            }
        }

        // 2. Se houverem skins para desbloquear, sorteia uma
        if (lockedSkins.Count > 0)
        {
            SkinData newSkin = lockedSkins[Random.Range(0, lockedSkins.Count)];
            CurrentPlayerData.unlockedSkinIDs.Add(newSkin.skinID);
            SaveCurrentProfileData();

            Debug.Log($"Jogador desbloqueou a skin: {newSkin.skinName}!");
            // Aqui você pode mostrar um painel de "Nova Skin Desbloqueada!"
        }
        else
        {
            Debug.Log("Jogador já tem todas as skins!");
        }
    }

    // Define a skin equipada
    public void EquipSkin(string skinID)
    {
        if (CurrentPlayerData == null || !CurrentPlayerData.unlockedSkinIDs.Contains(skinID))
        {
            Debug.LogError("Tentativa de equipar uma skin não desbloqueada!");
            return;
        }

        CurrentPlayerData.equippedSkinID = skinID;
        SaveCurrentProfileData();
        Debug.Log($"Skin equipada: {skinID}");
    }

    // Retorna o sprite da skin equipada atualmente
    public RuntimeAnimatorController GetEquippedSkinController()
    {
        if (CurrentPlayerData == null || skinDatabase == null)
        {
            Debug.LogError("SaveManager não está pronto ou não tem banco de dados de skins.");
            return null;
        }

        // Procura a skin no catálogo pelo ID salvo
        foreach (var skin in skinDatabase.allSkins)
        {
            if (skin.skinID == CurrentPlayerData.equippedSkinID)
            {
                return skin.animatorController;
            }
        }

        // Fallback: se não encontrar (ex: save corrompido), retorna a primeira skin padrão
        foreach (var skin in skinDatabase.allSkins)
        {
            if (skin.isUnlockedByDefault) return skin.animatorController;
        }

        return null;
    }

    public int GetInfinityHighScore(string profileName)
    {
        string filePath = GetSaveFilePath(profileName);
        if (!File.Exists(filePath))
        {
            return 0; // Perfil não existe ou não tem arquivo
        }

        try
        {
            // Lê o arquivo
            string json = File.ReadAllText(filePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            // Encontra o high score
            if (data.infinityModeScoreHistory != null && data.infinityModeScoreHistory.Count > 0)
            {
                // Usa Linq para encontrar o maior número na lista
                return data.infinityModeScoreHistory.Max();
            }

            return 0; // Lista de score vazia
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Falha ao ler high score do perfil '{profileName}': {e.Message}");
            return 0;
        }
    }

    public string GetCurrentProfileName()
    {
        if (CurrentProfileName != null)
            return CurrentProfileName;
        else return "";
    }

    public List<PlayerData> LoadAllProfilesForReport()
    {
        List<PlayerData> allProfiles = new List<PlayerData>();

        // 'saveFolderPath' is the variable you already use (e.g., ".../Profiles")
        if (!Directory.Exists(saveFolderPath))
        {
            Debug.LogWarning($"[SaveManager] Profiles folder '{saveFolderPath}' does not exist. No report to generate.");
            return allProfiles; // Return empty list
        }

        // Find all .json files that start with "profile_"
        string[] profileFiles = Directory.GetFiles(saveFolderPath, "profile_*.json");

        Debug.Log($"[SaveManager] Found {profileFiles.Length} profiles for the report.");

        // Read each file and convert it from JSON to PlayerData
        foreach (string filePath in profileFiles)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                PlayerData profileData = JsonUtility.FromJson<PlayerData>(json);

                if (profileData != null)
                {
                    allProfiles.Add(profileData);
                }
            }
            catch (System.Exception e)
            {
                // Protection so one corrupted file doesn't break the whole report
                Debug.LogError($"[SaveManager] Failed to read profile {filePath} for report. Error: {e.Message}");
            }
        }

        return allProfiles;
    }

}