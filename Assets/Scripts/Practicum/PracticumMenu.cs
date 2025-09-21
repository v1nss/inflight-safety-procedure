using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticumMenu : MonoBehaviour {

    [Header("Practicum Menu Buttons")]
    public Button airmaskButton;
    public Button seatbeltButton;
    public Button lifevestButton;
    public Button airSackButton;
    public Button exitDoorsButton;
    public Button quitButton;

    void Start()
    {
        airmaskButton.onClick.AddListener(StartAirMask);
        seatbeltButton.onClick.AddListener(StartSeatbelt);
        lifevestButton.onClick.AddListener(StartLifevest);
        airSackButton.onClick.AddListener(StartAirSack);
        exitDoorsButton.onClick.AddListener(StartExitDoors);
        quitButton.onClick.AddListener(QuitPracticum);
    }

    public void StartAirMask()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(3);
    }

    public void StartSeatbelt()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(5);
    }

    public void StartLifevest()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(2);
    }

    public void StartAirSack()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(4);
    }

    public void StartExitDoors()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(6);
    }

    public void QuitPracticum()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(0);
    }

}
