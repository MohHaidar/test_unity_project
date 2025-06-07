using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerLevelText;

    void Start()
    {
        playerNameText.text = PlayerPrefs.GetString("PlayerName", "Unknown Player");
        playerLevelText.text = "" + PlayerPrefs.GetInt("PlayerLevel", 1);
    }

}