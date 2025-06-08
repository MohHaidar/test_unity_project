using UnityEngine;
using UnityEngine.SceneManagement;
using Subjects.Math.Arithmetic_Challenges.HighNumbers;

public class ChallengeSelectMenu : MonoBehaviour {
    public void StartHighNumbers() {
        ChallengeLoader.SelectedChallenge = HighNumbersChallenge.Create();
        SceneManager.LoadScene("ChallengeScene");
    }
}
