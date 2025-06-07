using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void Scene_loader(int SceneIndex)
    {
        SceneManager.LoadScene(SceneIndex);
    }
    public void Scene_loader(string SceneName)
    {
        SceneManager.LoadScene(SceneIndex);
    }
}
