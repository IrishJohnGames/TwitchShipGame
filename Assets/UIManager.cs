using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ManagerBase<UIManager>
{
    [SerializeField]
    Popup popup;

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

    [ContextMenu("Popup!")]
    public void TestPopup()
    {
        ShowPopup("Helloooooo testt popup!");
    }

    internal void ShowPopup(string text)
    {
        popup.txtbox.text = text;
        popup.anim.Play("BounceInOut");
    }

}
