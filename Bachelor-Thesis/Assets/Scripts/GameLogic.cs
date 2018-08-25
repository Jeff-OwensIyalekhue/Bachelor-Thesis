using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour {

    [SerializeField]
    bool testing = false;

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
    float timeLimit = 0;
    public bool showTimer = true;
    public bool showScore = true;

    int numberA; 
    int numberB;
    int op;                         // operator: 0 := addition; 1 := subtraction; 2 := multiplication; else := error
    [SerializeField]
    int expResult;                  // expected result = numberA op numberB

    int entResult;                  // entered rusult

    bool answered = true;


	// Use this for initialization
	void Awake () {
        GameManager.Instance.gameRunning = true;
        if (timeLimit == 0)
            timer.text = "infinite";

        if (!showTimer)
            timer.gameObject.SetActive(false);

        if (!showScore)
            score.gameObject.SetActive(false);

        eventSystem = FindObjectOfType<EventSystem>();
    }
	
	// Update is called once per frame
	void Update () {
        if (timeLimit > 0 && showTimer && (timeLimit - Time.time) >= 0)
            timer.text = (timeLimit - Time.time).ToString("N0");
        if (timeLimit > 0 && (timeLimit - Time.time) <= 0)
        {
            GameManager.Instance.gameRunning = false;
            inputField.DeactivateInputField();
            task.text = "end";
        }
        if (GameManager.Instance.gameRunning)
        {
            if (answered)
            {
                Task();
            }
            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(inputField.gameObject);
            }
        }
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
