using Unity;
using UnityEditor;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject mainMenuRef;
    public GameObject gameMenuRef;
    public GameObject optionsMenuRef;
    public GameObject hostMenuRef;
    public GameObject lobbyMenuRef;

    #region MainMenu
    public void Play()
    {
        SwitchMenu(mainMenuRef, gameMenuRef);
    }

    public void OptionsMenu()
    {
        SwitchMenu(mainMenuRef, optionsMenuRef);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Game Menu
    public void HostMenu()
    {
        SwitchMenu(gameMenuRef, hostMenuRef);
    }

    public void LobbyMenu()
    {
        SwitchMenu(gameMenuRef, lobbyMenuRef);
    }
    #endregion

    public void SwitchMenu(GameObject currentMenu, GameObject newMenu)
    {
        currentMenu.SetActive(false);
        if (currentMenu.activeSelf == false)
        {
            newMenu.SetActive(true);
        }
    }
}