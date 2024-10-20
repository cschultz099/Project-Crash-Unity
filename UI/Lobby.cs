using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Lobby : NetworkBehaviour
{
    public void ReturnToMenu()
    {
        CustomNetworkManager.singleton.serverShutdownProcess();
    }

    public void MainStory()
    {
        GameManager.Singleton.MainCampaign();
    }

    public void EndlessMode()
    {
        GameManager.Singleton.EternalQuest();
    }
}
