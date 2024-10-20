using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public TMP_Text nameText;
    public Button joinButton;

    public void Setup(string name, System.Action onJoinClicked)
    {
        nameText.text = name;
        joinButton.onClick.AddListener(onJoinClicked.Invoke);
    }
}
