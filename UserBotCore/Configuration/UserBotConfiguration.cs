namespace UserBotCore.Configuration;

public class UserBotConfiguration
{
    public int ApiId { get; set; }
    public string ApiHash { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string SessionPath { get; set; } = "userbot.session";
}
