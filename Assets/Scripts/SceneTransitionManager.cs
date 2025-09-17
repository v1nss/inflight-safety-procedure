using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public static SceneTransitionManager singleton;

    private void Awake()
    {
        if (singleton && singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToScene(int sceneIndex)
    {
        if (fadeScreen == null) return;
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        yield return StartCoroutine(fadeScreen.FadeOut());
        SceneManager.LoadScene(sceneIndex);

        // Find fade screen in new scene if needed
        if (fadeScreen == null)
            fadeScreen = FindObjectOfType<FadeScreen>();

        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true); // Ensure it's active
            yield return StartCoroutine(fadeScreen.FadeIn());
        }
    }

    public void GoToSceneAsync(int sceneIndex)
    {
        if (fadeScreen == null) return;
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        yield return StartCoroutine(fadeScreen.FadeOut());

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        // Wait for loading to complete (90% means ready)
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;

        // Wait for scene to actually load
        while (!operation.isDone)
        {
            yield return null;
        }

        // Find fade screen in new scene if needed
        if (fadeScreen == null)
            fadeScreen = FindObjectOfType<FadeScreen>();

        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true); // Ensure it's active
            yield return StartCoroutine(fadeScreen.FadeIn());
        }
    }
}