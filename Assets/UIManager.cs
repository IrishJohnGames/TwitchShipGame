using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ManagerBase<UIManager>
{
    [SerializeField]
    Popup popup;

    [SerializeField]
    WinnerPopup winnerPopup;

    [SerializeField]
    GameObject BRInProgressCanvas;

    [System.Serializable]
    class Popup
    {
        [SerializeField]
        public GameObject obj;

        [SerializeField]
        public Animator anim;

        [SerializeField]
        public TMPro.TextMeshProUGUI txtbox;
    }

    [System.Serializable]
    class WinnerPopup
    {
        [SerializeField]
        public GameObject obj;

        [SerializeField]
        public Animator anim;

        [SerializeField]
        public TMPro.TextMeshProUGUI title, descript, crewlist;
    }

    [ContextMenu("Popup!")]
    public void TestPopup()
    {
        ShowPopup("Helloooooo testt popup!");
    }

    [ContextMenu("Winner Poopup!")]
    public void TestWinnerPopup()
    {
        ShowWinnerPopup("Winner is Horn Coom", "Description of the horn coom is going to go here", "Capn Jern, \nPoopDeckSwabber Hardold, \nFekker Arkraga ");
    }

    internal void ShowPopup(string text)
    {
        popup.txtbox.text = text;
        popup.anim.Play("BounceInOut");
    }

    internal void ShowWinnerPopup(string title, string description, string crewList)
    {
        winnerPopup.title.text = title;
        winnerPopup.crewlist.text = crewList;
        winnerPopup.descript.text = description;
        winnerPopup.anim.Play("BounceInOutWinner");
    }

    internal void ShowBRInProgressScreen() => BRInProgressCanvas.SetActive(true);
    internal void HideBRInProgressScreen() => BRInProgressCanvas.SetActive(false);
}
