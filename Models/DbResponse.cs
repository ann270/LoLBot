namespace TelegramBot;

public class DbResponse
{
    public string RequestID { get; set; }
    public string UserID { get; set; }
    public string StartTime { get; set; }
    public string BlockName { get; set; }
    public string LeagueName { get; set; }
    public List<string> MatchTeams { get; set; }
    public string MatchID { get; set; }
}