using UnityEngine;
using UnityEngine.UI;

public class AssessmentUIManager : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject assessmentMainMenu;
    public GameObject quiz;
    public GameObject result;

    [Header("Assessment MainMenu Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Result Buttons")]
    public Button retryButton;
    public Button resultQuitButton;

    [Header("Quiz Buttons")]
    public Button quizQuitButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartQuiz);
        quitButton.onClick.AddListener(QuitAssessment);
        retryButton.onClick.AddListener(RetryQuiz);
        resultQuitButton.onClick.AddListener(ResultQuitButton);
        quizQuitButton.onClick.AddListener(QuizQuitButton);
    }

    public void StartQuiz()
    {
        assessmentMainMenu.SetActive(false);
        quiz.SetActive(true);
    }

    public void QuitAssessment()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(0);
    }

    public void RetryQuiz()
    {
        result.SetActive(false);
        quiz.SetActive(true);
    }

    public void ResultQuitButton()
    {
        result.SetActive(false);
        assessmentMainMenu.SetActive(true);
    }

    public void QuizQuitButton()
    {
        quiz.SetActive(false);
        assessmentMainMenu.SetActive(true);
    }

}
