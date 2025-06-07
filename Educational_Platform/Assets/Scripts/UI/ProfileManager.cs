using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerLevelText;
    [SerializeField] private Text playerAchievementsText;
    [SerializeField] private Image playerAvatarImage;

    void Start()
    {
        playerNameText.text = PlayerPrefs.GetString("PlayerName", "Unknown Player");
        playerLevelText.text = "Level: " + PlayerPrefs.GetInt("PlayerLevel", 1);
        playerAchievementsText.text = PlayerPrefs.GetString("PlayerAchievements", "No Achievements");

        string avatarPath = PlayerPrefs.GetString("PlayerAvatar", "");
        if (!string.IsNullOrEmpty(avatarPath))
        {
            Texture2D avatarTexture = LoadTextureFromPath(avatarPath);
            if (avatarTexture != null)
            {
                playerAvatarImage.sprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), Vector2.one * 0.5f);
            }
        }
    }

    private Texture2D LoadTextureFromPath(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
}
