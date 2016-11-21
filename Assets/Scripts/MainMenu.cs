using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    public GameObject instructionsPanel;
    public GameObject startButton;
    public GameObject hangmanInitialImage;
    public GameObject endGameOverlayPanel;
    private EndGamePopupController endGamePopupController;

    public GameObject wordContainer;
    public GameObject letterPrefab;
    public GameObject keyboardContainer;

    public AudioClip keyPressSound;
    public AudioClip defeatSound;
    public AudioClip victorySound;
    private AudioSource audioSource;

    public GameObject[] hangParts;
    private int missesCount;
    private int hitsCount;

    const string LETTERS_STRING = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private List<string> wordsList;
    private string currentWord;
    private int victoriesCount;
    private int defeatsCount;


	// Use this for initialization
	void Start () {
        victoriesCount = 0;
        defeatsCount = 0;
        LoadWordsList();
        ResetVariables();

        audioSource = GetComponent<AudioSource>();
        endGamePopupController = endGameOverlayPanel.GetComponent<EndGamePopupController>();
	}

    private void ResetVariables() {
        missesCount = 0;
        hitsCount = 0;
        currentWord = "";
    }

    private void LoadWordsList() {
        TextAsset file = Resources.Load<TextAsset>("wordsList");
        wordsList = new List<string>(file.text.Split('\n'));
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Play() {
        HideInstructions();

        StartCoroutine(FillWordContainer());

        StartCoroutine(InitializeKeyboard());
    }    

    public void Restart() {
        audioSource.Stop();

        foreach (Transform letterTransform in wordContainer.transform)
        {
            if (letterTransform.gameObject.activeInHierarchy) {
                Destroy(letterTransform.gameObject);
            }
        }       

        HideHangman();
        ResetVariables();
        endGameOverlayPanel.SetActive(false);

        StartCoroutine(FillWordContainer());

        ResetKeyboard();
    }

    private void HideInstructions() {
        SetAlphaToZero(hangmanInitialImage);

        instructionsPanel.SetActive(false);
        startButton.SetActive(false);
    }

    private void SetAlphaToZero(GameObject obj) {
        if (obj.GetComponent<Image>() != null)
        {
            obj.GetComponent<Image>().CrossFadeAlpha(0.001f, 1f, true);
        }
        else 
        {
            StartCoroutine(FadeTo(obj.transform, 0f, 1f));
        }
    }

    IEnumerator FadeTo(Transform transform, float aValue, float aTime)
    {
        float alpha = transform.GetComponent<Renderer>().material.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha,aValue,t));
            transform.GetComponent<Renderer>().material.color = newColor;
            yield return null;
        }
    }

    IEnumerator FillWordContainer()
    {
        float letterWidth = 75f;

        currentWord = wordsList[Random.Range(0, wordsList.Count)];
        //wordsList.Remove(currentWord);

        float wordContainerSizeX = wordContainer.GetComponent<RectTransform>().rect.width;
        // screen size minus a letter's width so it doesn't hide the letters at the left/rightmost positions
        float wordContainerMinusLetters = (wordContainerSizeX - letterWidth) - (letterWidth * currentWord.Length);
        float spaceBetweenLetters = wordContainerMinusLetters / (currentWord.Length + 1);
        // starting from the leftmost point + half a letter's length
        float prevLetterX = -(wordContainerSizeX / 2) + (letterWidth / 2);

        float lettersSpace = currentWord.Length * letterWidth;
        float startingPoint = ((wordContainerSizeX - lettersSpace) / 2f) - (wordContainerSizeX / 2f);


        for (int i = 0; i < currentWord.Length; i++) {
            float letterX = prevLetterX + spaceBetweenLetters + (letterWidth / 2);

            GameObject newLetter = Instantiate(letterPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            newLetter.transform.SetParent(wordContainer.transform, true);

            newLetter.transform.localPosition = new Vector3(letterX, 0, 0);
            prevLetterX = letterX + (letterWidth / 2);

            newLetter.transform.localScale = new Vector3(1, 1, 1);
            Text newLetterText = newLetter.GetComponentInChildren<Text>();
            newLetterText.text = currentWord[i].ToString();
            newLetterText.gameObject.SetActive(false);
            newLetter.SetActive(true);

            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator InitializeKeyboard() {        
        keyboardContainer.SetActive(true);
        Transform child = keyboardContainer.transform.GetChild(0);

        for (int i = 0; i < LETTERS_STRING.Length; i++)
        {
            GameObject newButton = Instantiate(child.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
            newButton.transform.SetParent(keyboardContainer.transform);
            newButton.transform.localScale = new Vector3(1, 1, 1);
            Text newButtonText = newButton.GetComponentInChildren<Text>();
            newButtonText.text = LETTERS_STRING[i].ToString();
            newButton.SetActive(true);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void ResetKeyboard() {
        foreach (Transform key in keyboardContainer.transform)
        {
            key.gameObject.GetComponentInChildren<Button>().enabled = true;
        }
    }

    public void KeyPressed() {
        GameObject pressedButton = EventSystem.current.currentSelectedGameObject;
        Text buttonText = pressedButton.GetComponentInChildren<Text>();
        PlayAudioClip(keyPressSound);
        CheckPressedLetter(buttonText.text);
        pressedButton.GetComponentInChildren<Button>().enabled = false;
    }

    private void CheckPressedLetter(string letter) {
        bool anyHits = false;

        foreach (Transform letterTransform in wordContainer.transform)
        {
            Text wordLetterText = letterTransform.gameObject.GetComponentInChildren(typeof(Text), true) as Text;
            if (wordLetterText && wordLetterText.text.ToUpper() == letter.ToUpper())
            {
                wordLetterText.gameObject.SetActive(true);
                anyHits = true;
                hitsCount++;
            }
        }

        if (anyHits) {     
            if (hitsCount >= currentWord.Length) {
                Victory();
            }
        } else {
            DisplayHangPart();
        }
    }

    private void DisplayHangPart() {
        if (missesCount < hangParts.Length - 1)
        {
            hangParts[missesCount].SetActive(true);
        } 
        else if (missesCount < hangParts.Length)
        {
            hangParts[missesCount].SetActive(true);
            Defeat();
        }

        missesCount++;
    }

    private void HideHangman() {
        foreach (GameObject part in hangParts)
        {
            part.SetActive(false);
        }
    }

    private void Victory() {
        PlayAudioClip(victorySound);
        ResetKeyboard();
        victoriesCount++;
        endGamePopupController.DisplayPopup("You Win!", "Victories: " + victoriesCount, "Defeats: " + defeatsCount);
        endGameOverlayPanel.SetActive(true);
    }

    private void Defeat() {
        PlayAudioClip(defeatSound);
        defeatsCount++;
        endGamePopupController.DisplayPopup("You Lose!", "Victories: " + victoriesCount, "Defeats: " + defeatsCount);
        endGameOverlayPanel.SetActive(true);
    }

    private void PlayAudioClip(AudioClip clip) {
        audioSource.clip = clip;
        audioSource.Play();
    } 
}
