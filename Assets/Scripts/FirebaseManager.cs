using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public static FirebaseAuth Auth;
    public static FirebaseFirestore DB;
    public static FirebaseUser User;

    async void Awake()
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

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            DB = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firebase inicializado com sucesso!");
        }
        else
        {
            Debug.LogError($"Falha ao inicializar o Firebase: {dependencyStatus}");
        }
    }


    public async Task<FirebaseUser> RegisterUser(string email, string password)
    {
        var authResult = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        User = authResult.User;
        Debug.LogFormat("Usuário criado com sucesso: {0} ({1})", User.DisplayName, User.UserId);

        await CreateInitialPlayerData();
        return User;
    }

    public async Task<FirebaseUser> LoginUser(string email, string password)
    {
        var authResult = await Auth.SignInWithEmailAndPasswordAsync(email, password);
        User = authResult.User;
        Debug.LogFormat("Usuário logado com sucesso: {0} ({1})", User.DisplayName, User.UserId);
        return User;
    }

    public void Logout()
    {
        if (Auth.CurrentUser != null)
        {
            Auth.SignOut();
            User = null;
            Debug.Log("Usuário deslogado.");
        }
    }

    public async Task CreateInitialPlayerData()
    {
        if (User == null) return;

        DocumentReference playerDocRef = DB.Collection("players").Document(User.UserId);

        var initialData = new Dictionary<string, object>
        {
            { "email", User.Email },
            { "infinityModeHighScore", 0 },
            { "storyModeProgress", 0 },
            { "lastLogin", Timestamp.GetCurrentTimestamp() }
        };

        await playerDocRef.SetAsync(initialData);
        Debug.Log("Dados iniciais do jogador criados no Firestore.");
    }

    public async Task SavePlayerData(string dataType, object data)
    {
        if (User == null)
        {
            Debug.LogError("Nenhum usuário logado para salvar dados.");
            return;
        }

        DocumentReference playerDocRef = DB.Collection("players").Document(User.UserId);

        var updates = new Dictionary<string, object>
        {
            { dataType, data }
        };

        await playerDocRef.UpdateAsync(updates);
        Debug.Log($"Dados do jogador salvos: {dataType}");
    }

    public async Task SaveStoryModeResult(string playerName, List<PhaseResult> results)
    {
        string userId = User != null ? User.UserId : "anonymous";

        List<Dictionary<string, object>> phaseDataList = new List<Dictionary<string, object>>();
        foreach (var result in results)
        {
            var phaseData = new Dictionary<string, object>
        {
            { "phaseName", result.phaseName },
            { "score", result.score },
            { "timeTaken", result.timeTaken }
        };
            phaseDataList.Add(phaseData);
        }

        var dataToSave = new Dictionary<string, object>
    {
        { "playerName", playerName },
        { "userId", userId },
        { "timestamp", Timestamp.GetCurrentTimestamp() },
        { "phases", phaseDataList }
    };

        await DB.Collection("storyModeResults").AddAsync(dataToSave);

        Debug.Log("Story Mode results saved to Firestore!");
    }
}