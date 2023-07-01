namespace TelegramBot;

public class ChampionSkinsResponse
{
    public List<Champion> Champion { get; set; }
}

public class Champion
{
    public List<Skins> Skins { get; set; }
}

public class Skins
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
}