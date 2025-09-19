using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject safetyProcedures;
    public GameObject options;
    public GameObject about;

    [Header("Main Menu Buttons")]
    public Button startButton;
    //public Button optionButton;
    public Button aboutButton;
    public Button quitButton;

    [Header("Safety Procedure Buttons")]
    public Button safetyDemonstration;
    public Button practicum;
    public Button assessment;
    //public Button quitButton;

    public List<Button> returnButtons;

    // Start is called before the first frame update
    void Start()
    {
        EnableMainMenu();

        //Hook events
        startButton.onClick.AddListener(StartGame);
        //optionButton.onClick.AddListener(EnableOption);
        aboutButton.onClick.AddListener(EnableAbout);
        quitButton.onClick.AddListener(QuitGame);

        safetyDemonstration.onClick.AddListener(StartSafetyDemonstration);
        practicum.onClick.AddListener(StartPracticum);
        assessment.onClick.AddListener(StartAssessment);

        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        HideAll();
        safetyProcedures.SetActive(true);
        //SceneTransitionManager.singleton.GoToSceneAsync(1);
    }

    public void HideAll()
    {
        mainMenu.SetActive(false);
        //safetyProcedures.SetActive(false);
        options.SetActive(false);
        about.SetActive(false);
    }

    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        options.SetActive(false);
        about.SetActive(false);
    }
    public void EnableOption()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
        about.SetActive(false);
    } 
    public void EnableAbout()
    {
        mainMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(true);
    }

    public void StartPracticum()
    {
        HideAll();
        safetyProcedures.SetActive(false);
        SceneTransitionManager.singleton.GoToSceneAsync(1);
    }
    public void StartSafetyDemonstration()
    {
        return;
    }
    public void StartAssessment()
    {
        HideAll();
        safetyProcedures.SetActive(false);
        SceneTransitionManager.singleton.GoToSceneAsync(7);
    }
}
