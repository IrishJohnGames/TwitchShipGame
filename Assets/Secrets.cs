using System.IO;

public class Secrets
{
    const string PATH_TO_SECRET_INFO = @"K:\OAUTH\Secrets\";
    public string

        // https://dev.twitch.tv/
        client_id = "", 
        client_secret = "",

        // from https://twitchtokengenerator.com/
        bot_access_token = "", 
        bot_refresh_token = "",

        // Needs channel:read:redemptions & chat_login & openid
        oauth_redemption = "";

    public string john_id = "", jern_id = "";

    public Secrets()
    {
        client_id = File.ReadAllText(PATH_TO_SECRET_INFO + "clientid.txt");
        client_secret = File.ReadAllText(PATH_TO_SECRET_INFO + "clientsecret.txt");
        bot_access_token = File.ReadAllText(PATH_TO_SECRET_INFO + "botaccesstoken.txt");
        bot_refresh_token = File.ReadAllText(PATH_TO_SECRET_INFO + "botrefreshtoken.txt");
        oauth_redemption = File.ReadAllText(PATH_TO_SECRET_INFO + "oauth_redemptionreading.txt");

        // I used this extension for these: Twitch Username and User ID Translator (on chrome)
        john_id = File.ReadAllText(PATH_TO_SECRET_INFO + "irish_john_id.txt");
        jern_id = File.ReadAllText(PATH_TO_SECRET_INFO + "jern_id.txt"); // The jerns are my bots
    }
}