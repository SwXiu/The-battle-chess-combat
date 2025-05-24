using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }

    [SerializeField] private Animator menuAnimator;

    private void start()
    {
        Instance = this;
        Time.timeScale = 0f;
    }

    public void OnPlayButton()
    {
        menuAnimator.SetTrigger("PlayMenu");
    }

    public void OnSettingsButton()
    {
        menuAnimator.SetTrigger("SettingMenu");
    }

    public void OnCreditsButton()
    {
        menuAnimator.SetTrigger("CreditsMenu");
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnLocalButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        Time.timeScale = 1f;
    }

    public void OnOnlineButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnHostButton()
    {
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnBackButton()
    {
        menuAnimator.SetTrigger("GameMenu");
    }
}
