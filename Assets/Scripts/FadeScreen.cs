using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool fadeOnStart = true;
    public float fadeDuration = 2f;
    public Color fadeColor;
    public AnimationCurve fadeCurve;
    public string colorPropertyName = "_BaseColor";
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;

        if (fadeOnStart)
        {
            // Make sure GameObject is active before starting coroutine
            gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0, 1));
    }

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1, 0));
    }

    private IEnumerator Fade(float alphaIn, float alphaOut)
    {
        rend.enabled = true;

        float timer = 0f;
        while (timer <= fadeDuration)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, fadeCurve.Evaluate(timer / fadeDuration));
            rend.material.SetColor(colorPropertyName, newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        // Make sure final alpha is applied
        Color finalColor = fadeColor;
        finalColor.a = alphaOut;
        rend.material.SetColor(colorPropertyName, finalColor);

        if (alphaOut == 0)
            rend.enabled = false; // hide plane if fully transparent
    }
}