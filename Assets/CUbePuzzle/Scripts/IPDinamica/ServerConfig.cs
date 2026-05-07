using UnityEngine;

public static class ServerConfig
{
    private static string ip = "localhost";

    public static string IP
    {
        get
        {
            if (PlayerPrefs.HasKey("SERVER_IP"))
            {
                ip = PlayerPrefs.GetString("SERVER_IP");
            }

            return ip;
        }

        set
        {
            ip = value;
            PlayerPrefs.SetString("SERVER_IP", ip);
            PlayerPrefs.Save();
        }
    }

    public static string BaseUrl
    {
        get
        {
            return $"http://{IP}:5005/server";
        }
    }
}