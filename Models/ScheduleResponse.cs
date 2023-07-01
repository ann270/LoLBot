namespace TelegramBot;

public class ScheduleResponse
{
    public Data Data { get; set; }
}

public class Data
{
    public Schedule Schedule { get; set; }
}

public class Schedule
{
    public List<Events> Events { get; set; }
}

public class Events
{
    public string StartTime { get; set; }
    public string BlockName { get; set; }
    public League League { get; set; }
    public Match Match { get; set; }

}

public class League
{
    public string Name { get; set; }
}

public class Match
{
    public List<Teams> Teams { get; set; }
    public string Id { get; set; }
}

public class Teams
{
    public string Name { get; set; }
    
}