using UnityEngine;
using System.Collections.Generic;
using System.IO;                // Para salvar arquivos
using System.Linq;              // Para usar .Max() e .OrderBy()
using System.Text;              // Para o StringBuilder (MUITO importante)

// Este é o script que você vai colocar no seu objeto "ReportSystem"
public class ReportExporter : MonoBehaviour
{
    // ======================================================================
    // FUNÇÃO DE BUSCAR DADOS (Igual a antes, perfeita)
    // ======================================================================
    private List<PlayerData> GetAllPlayerData()
    {
        Debug.Log("Fetching REAL data from SaveManager...");
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance is null! Report cannot be generated.");
            return new List<PlayerData>();
        }
        return SaveManager.Instance.LoadAllProfilesForReport();
    }

    // ======================================================================
    // NOVA FUNÇÃO DO BOTÃO (Gerar HTML)
    // ======================================================================
    public void GenerateHtmlReport()
    {
        Debug.Log("Starting HTML report generation...");

        // 1. OBTER OS DADOS
        List<PlayerData> allPlayerData = GetAllPlayerData();
        if (allPlayerData == null || allPlayerData.Count == 0)
        {
            Debug.LogWarning("No player data found to export.");
            return;
        }

        // 2. CONSTRUIR A STRING HTML
        // Usamos StringBuilder, que é muito eficiente para montar strings longas
        StringBuilder sb = new StringBuilder();

        // Cabeçalho do HTML e o CSS "bonitinho"
        sb.Append("<!DOCTYPE html><html lang=\"pt-br\"><head>");
        sb.Append("<meta charset=\"UTF-8\">");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.Append("<title>Relatório de Pontuação - Numener</title>");
        sb.Append(GetCssStyle()); // Pega o bloco de CSS da função abaixo
        sb.Append("</head><body>");

        // Corpo do HTML
        sb.Append("<div class=\"container\">");
        sb.Append("<h1>Relatório de Pontuação - Numener</h1>");

        // Loop por cada jogador
        foreach (var playerData in allPlayerData.OrderBy(p => p.playerName)) // Ordena por nome
        {
            sb.Append("<div class=\"player-card\">");
            sb.Append($"<h2>{playerData.playerName}</h2>");

            // --- Info: Modo Infinito ---
            int highScore = 0;
            if (playerData.infinityModeScoreHistory != null && playerData.infinityModeScoreHistory.Count > 0)
            {
                highScore = playerData.infinityModeScoreHistory.Max();
            }
            sb.Append($"<p><strong>Recorde Modo Infinito:</strong> {highScore}</p>");

            // --- Tabela: Modo História ---
            if (playerData.storyModeResults != null && playerData.storyModeResults.Count > 0)
            {
                sb.Append("<h3>Resultados - Modo História</h3>");
                sb.Append("<table><thead><tr><th>Fase</th><th>Pontos</th><th>Tempo</th></tr></thead><tbody>");

                // Loop por cada fase, ordenado pelo índice
                foreach (var phase in playerData.storyModeResults.OrderBy(p => p.phaseIndex))
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{phase.phaseName} (Fase {phase.phaseIndex + 1})</td>");
                    sb.Append($"<td>{phase.score}</td>");
                    sb.Append($"<td>{phase.timeTaken} segundos</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
            }
            else
            {
                sb.Append("<p>Nenhum resultado no Modo História.</p>");
            }
            sb.Append("</div>"); // Fim do .player-card
        }

        sb.Append("</div>"); // Fim do .container
        sb.Append("</body></html>");

        // 3. SALVAR O ARQUIVO HTML
        try
        {
            string htmlString = sb.ToString();
            string fileName = "relatorio_pontuacao.html"; // Mude a extensão!
            string path = Path.Combine(Application.persistentDataPath, fileName);

            File.WriteAllText(path, htmlString, Encoding.UTF8); // Força UTF-8 para acentos

            Debug.Log($"Report saved successfully at: {path}");

            // ABRIR A PASTA!
            Application.OpenURL(Application.persistentDataPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save HTML file: {ex.Message}");
        }
    }

    // ======================================================================
    // FUNÇÃO DO CSS (Sinta-se livre para editar!)
    // ======================================================================
    private string GetCssStyle()
    {
        // Usamos @"" para criar uma string de múltiplas linhas
        return @"
<style>
    body {
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
        background-color: #f0f2f5;
        color: #333;
        margin: 0;
        padding: 20px;
    }
    .container {
        max-width: 900px;
        margin: 0 auto;
        background-color: #ffffff;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
        padding: 30px;
    }
    h1 {
        text-align: center;
        color: #1a237e;
        border-bottom: 2px solid #3949ab;
        padding-bottom: 10px;
    }
    .player-card {
        background-color: #fafafa;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        padding: 20px;
        margin-bottom: 20px;
    }
    h2 {
        color: #3949ab;
        margin-top: 0;
    }
    h3 {
        color: #555;
        border-bottom: 1px solid #ccc;
        padding-bottom: 5px;
    }
    p {
        font-size: 16px;
    }
    table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 15px;
    }
    th, td {
        border: 1px solid #ddd;
        padding: 10px;
        text-align: left;
    }
    th {
        background-color: #3949ab;
        color: white;
        font-weight: bold;
    }
    tr:nth-child(even) {
        background-color: #f9f9f9;
    }
</style>
";
    }
}