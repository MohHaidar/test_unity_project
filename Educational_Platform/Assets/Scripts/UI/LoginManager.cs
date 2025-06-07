using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private InputField playerNameInput;

    public void LoginPlayer()
    {
        string playerName = playerNameInput.text;
        PlayerCSVManager.LoadPlayerToMemory(playerName);

        if (playerName == PlayerPrefs.GetString("PlayerName", ""))
        {
            Debug.Log("Player data loaded.");
            SceneManager.LoadScene("MainMenu");
        }
        else
            Debug.LogError("Player not found!");
    }
}
