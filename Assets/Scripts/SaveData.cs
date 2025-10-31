using System.Collections.Generic;
using System; // Necessário para [Serializable]

// SaveData.cs - Define a estrutura dos dados salvos.
// Não é um MonoBehaviour.

[Serializable] // Permite que o Unity salve/carregue esta classe em JSON
public class PlayerData
{
    public string playerName;
    public List<PhaseResult> storyModeResults;
    public List<int> infinityModeScoreHistory;

    public string equippedSkinID;          // O ID da skin que o jogador está usando
    public List<string> unlockedSkinIDs;   // A lista de IDs de skins que o jogador possui
}

[Serializable] // A estrutura para guardar o resultado de cada fase do Modo História
public class PhaseResult
{
    public int phaseIndex;
    public string phaseName;
    public int score;
    public float timeTaken;
}