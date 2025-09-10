using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneCycler : MonoBehaviour
{

    [Header("Scene Cycler Buttons")]
    public Button quitButton;
    public Button nextButton;

    // Range of scenes you want to cycle through
    private int minIndex = 2;
    private int maxIndex = 6;


    void Start()
    {
        quitButton.onClick.AddListener(QuitScene);
        nextButton.onClick.AddListener(LoadNextScene);
    }

    public void QuitScene()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(1);
    }


    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;

        // If current is before the range, jump to start
        if (currentIndex < minIndex || currentIndex > maxIndex)
        {
            SceneManager.LoadScene(minIndex);
            return;
        }

        // Move to next scene in range
        int nextIndex = currentIndex + 1;

        // Loop back to start if past the max
        if (nextIndex > maxIndex)
        {
            nextIndex = minIndex;
        }

        SceneManager.LoadScene(nextIndex);
    }
}
