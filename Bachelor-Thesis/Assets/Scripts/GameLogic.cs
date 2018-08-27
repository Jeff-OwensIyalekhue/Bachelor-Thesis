using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour {

    [Header("UI")]
    public TMP_Text timer;
    public TMP_Text score;
    public TMP_Text task;
    public TMP_InputField inputField;
    EventSystem eventSystem;

    [Header("Settings")]
    public int intervalMin = 0;
    public int intervalMax = 10;    // exclusive
    [SerializeField]
    float timeLimit = 0;            // if value <= 0: no time limit
    public bool showTimer = true;
    public bool showScore = true;

    int numberA; 
    int numberB;
    int op;                         // operator: 0 := addition; 1 := subtraction; 2 := multiplication; else := error
    [SerializeField]
    int expResult;                  // expected result = numberA op numberB

    int entResult;                  // entered rusult

    bool answered = true;
    float timeStart;


	// Use this for initialization
	void Awake () {
        GameManager.Instance.gameRunning = true;
        
        // if timeLimit is set is set to a "invalid time" set, show
        if (timeLimit <= 0)
            timer.text = "no time limit";

        if (!showTimer)
            timer.gameObject.SetActive(false);
        timeStart = Time.time;

        if (!showScore)
            score.gameObject.SetActive(false);

        eventSystem = FindObjectOfType<EventSystem>();
    }
	
	// Update is called once per frame
	void Update () {
        
        // display the remaining time if a time limit is set
        if (timeLimit > 0 && showTimer && (timeLimit + timeStart - Time.time) >= 0)
            timer.text = (timeLimit + timeStart - Time.time).ToString("N0");

        // if time limit is reached ends the game and returns to menu
        if (timeLimit > 0 && (timeLimit + timeStart - Time.time) <= 0)
        {
            GameManager.Instance.gameRunning = false;
            inputField.DeactivateInputField();
            task.text = "end";
            StartCoroutine(End());
        }

        // if there is no time limit, end the game when escape is pressed
        if(timeLimit <= 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.gameRunning = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        // checks new task should be asked
        if (GameManager.Instance.gameRunning)
        {
            if (answered)
            {
                Task();
            }

            // ensures that the inputfield is selected by the event system
            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(inputField.gameObject);
            }
        }
    }

    // returns to the menu with a little delay
    IEnumerator End()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // Sets a new task up
    void Task()
    {
        if(showScore)
            score.text = "Correct:" + GameManager.Instance.correctAnswers + " Skipped:" + GameManager.Instance.skippedAnswers + " Wrong:" + GameManager.Instance.wrongAnswers;

        answered = false;
        numberA = Random.Range(intervalMin, intervalMax);
        numberB = Random.Range(intervalMin, intervalMax);
        op = Random.Range(0, 3);

        switch (op)
        {
            case 0:
                expResult = numberA + numberB;
                task.text = numberA + " <color=green> + </color> " + numberB;
                break;
            case 1:
                expResult = numberA - numberB;
                task.text = numberA + " <color=red> - </color> " + numberB;
                break;
            case 2:
                expResult = numberA * numberB;
                task.text = numberA + " <color=blue> * </color> " + numberB;
                break;
            default:
                Debug.Log("unexpected value as operaor");
                break;
        }
    }

    public void Solve()
    {
        if (!GameManager.Instance.gameRunning)
            return;

        if (inputField.text.Length != 0)
        {
            entResult = int.Parse(inputField.text);
            if (entResult == expResult)
            {
                GameManager.Instance.correctAnswers++;
            }
            else
            {
                GameManager.Instance.wrongAnswers++;
            }
        }
        else
        {
            GameManager.Instance.skippedAnswers++;
        }
        
        inputField.DeactivateInputField();
        inputField.text = "";
        inputField.ActivateInputField();

        Task();
    }
}
