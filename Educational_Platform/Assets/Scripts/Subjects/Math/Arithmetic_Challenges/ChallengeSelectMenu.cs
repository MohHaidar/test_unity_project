using UnityEngine;
using UnityEngine.SceneManagement;
using Subjects.Math.Arithmetic_Challenges.BigNumberAddition;
using Subjects.Math.Arithmetic_Challenges.Subtraction;
using Subjects.Math.Arithmetic_Challenges.Multiplication;
using Subjects.Math.Arithmetic_Challenges.NegativeNumbers;

public class ChallengeSelectMenu : MonoBehaviour {
    public void StartBigNumberAddition() {
        ChallengeLoader.SelectedChallenge = BigNumberAdditionChallenge.Create();
        SceneManager.LoadScene("ChallengeScene");
    }

    public void StartSubtraction() {
        ChallengeLoader.SelectedChallenge = SubtractionChallenge.Create();
        SceneManager.LoadScene("ChallengeScene");
    }

    public void StartMultiplication() {
        ChallengeLoader.SelectedChallenge = MultiplicationChallenge.Create();
        SceneManager.LoadScene("ChallengeScene");
    }

    public void StartNegativeNumbers() {
        ChallengeLoader.SelectedChallenge = NegativeNumbersChallenge.Create();
        SceneManager.LoadScene("ChallengeScene");
    }
}
