using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndGamePopupController : MonoBehaviour {

    public GameObject restartButton;
    public Text titleText;
    public Text victoriesText;
    public Text defeatsText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DisplayPopup(string result, string victories, string defeats) {
        titleText.text = result;
        victoriesText.text = victories;
        defeatsText.text = defeats;
    }
}
