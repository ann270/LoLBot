namespace TelegramBot;

public class PlayerDetailsResponse
{
    public string Username { get; set; }
    public string Rank { get; set; }
    public string Lp { get; set; }
    public string WinLossRatio { get; set; }
    public List<MostPlayedChamps> MostPlayedChamps { get; set; }
}

public class MostPlayedChamps
{
    public string Name { get; set; }
    public string WinPercentage { get; set; }
    public string TotalGames { get; set; }
}