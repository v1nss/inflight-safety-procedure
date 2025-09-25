using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemonstrationUIManager : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenuButtons;
    public GameObject videosUI;
    public GameObject oxygenMaskUI;
    public GameObject seatbeltUI;
    public GameObject lifevestUI;
    public GameObject vomitBagUI;
    public GameObject emergencyExitUI;

    [Header("Practicum Menu Buttons")]
    public Button airmaskButton;
    public Button seatbeltButton;
    public Button lifevestButton;
    public Button airSackButton;
    public Button exitDoorsButton;
    public Button quitButton;

    public List<Button> returnButtons;

    void Start()
    {
        airmaskButton.onClick.AddListener(StartAirMask);
        seatbeltButton.onClick.AddListener(StartSeatbelt);
        lifevestButton.onClick.AddListener(StartLifevest);
        airSackButton.onClick.AddListener(StartAirSack);
        exitDoorsButton.onClick.AddListener(StartExitDoors);
        quitButton.onClick.AddListener(QuitDemonstration);

        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void EnableMainMenu()
    {
        Debug.Log("Quit Button Clicked");
        videosUI.SetActive(false);
        oxygenMaskUI.SetActive(false);
        seatbeltUI.SetActive(false);
        lifevestUI.SetActive(false);
        vomitBagUI.SetActive(false);
        emergencyExitUI.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    public void StartAirMask()
    {
        mainMenuButtons.SetActive(false);
        videosUI.SetActive(true);
        oxygenMaskUI.SetActive(true);
    }

    public void StartSeatbelt()
    {
        mainMenuButtons.SetActive(false);
        videosUI.SetActive(true);
        seatbeltUI.SetActive(true);
    }

    public void StartLifevest()
    {
        mainMenuButtons.SetActive(false);
        videosUI.SetActive(true);
        lifevestUI.SetActive(true);
    }

    public void StartAirSack()
    {
        mainMenuButtons.SetActive(false);
        videosUI.SetActive(true);
        vomitBagUI.SetActive(true);
    }

    public void StartExitDoors()
    {
        mainMenuButtons.SetActive(false);
        videosUI.SetActive(true);
        emergencyExitUI.SetActive(true);
    }

    public void QuitDemonstration()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(0);
    }

}
