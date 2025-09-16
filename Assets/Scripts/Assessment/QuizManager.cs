using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;
    public GameObject quizPanel;
    public GameObject resultPanel;

    [Header("Questions")]
    public List<QuestionData> allQuestions = new List<QuestionData>();
    private List<QuestionData> selectedQuestions = new List<QuestionData>();

    private int currentQuestionIndex = 0;
    private int score = 0;

    void OnEnable()
    {
        currentQuestionIndex = 0;
        score = 0;
        selectedQuestions.Clear();
        // Pick 10 random questions out of 20
        selectedQuestions = GetRandomQuestions(allQuestions, 10);

        DisplayQuestion();
    }

    void OnDisable()
    {
        Debug.Log("Disabled :(");
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            EndQuiz();
            return;
        }

        QuestionData q = selectedQuestions[currentQuestionIndex];
        questionText.text = q.questionText;

        //scoreText.text = "Your Score: " + score + " / " + selectedQuestions.Count;

        Debug.Log("Showing question: " + q.questionText);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // capture local copy
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
        Debug.Log($"HEHE {currentQuestionIndex}");
    }

    void OnAnswerSelected(int index)
    {
        QuestionData q = selectedQuestions[currentQuestionIndex];

        if (index == q.correctAnswerIndex)
        {
            score++;
            Debug.Log($"HI {score}");
        }

        currentQuestionIndex++;
        DisplayQuestion();
    }

    void EndQuiz()
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);
        scoreText.text = "Your Score: " + score + " / " + selectedQuestions.Count;
    }

    List<QuestionData> GetRandomQuestions(List<QuestionData> source, int count)
    {
        List<QuestionData> copy = new List<QuestionData>(source);
        List<QuestionData> result = new List<QuestionData>();

        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(0, copy.Count);
            result.Add(copy[rand]);
            copy.RemoveAt(rand);
        }

        return result;
    }
}
