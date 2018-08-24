using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour {

    [SerializeField]
    bool testing = false;

    [Header("UI")]
    public TMP_Text score;
    public TMP_Text task;
    public TMP_InputField inputField;
    EventSystem eventSystem;

    [Header("Settings")]
    public int intervalMin = 0;
    public int intervalMax = 10;    // exclusive

    int numberA; 
    int numberB;
    int op;                         // operator: 0 := addition; 1 := subtraction; 2 := multiplication; else := error
    [SerializeField]
    int expResult;                  // expected result = numberA op numberB

    int entResult;                  // entered rusult

    bool answered = true;


	// Use this for initialization
	void Start () {
        eventSystem = FindObjectOfType<EventSystem>();
    }
	
	// Update is called once per frame
	void Update () {

        if (answered || (testing && Input.GetKeyDown(KeyCode.Escape)))
        {
            Task();
        }
        if (!eventSystem.alreadySelecting)
        {
            eventSystem.SetSelectedGameObject(inputField.gameObject);
        }
	}

    // Sets a new task up
    void Task()
    {
        score.text = "Correct: " + GameManager.Instance.correctAnswers + " - Wrong:" + GameManager.Instance.wrongAnswers;

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
        if (inputField.text != null)
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
        
        inputField.DeactivateInputField();
        inputField.text = "";
        inputField.ActivateInputField();

        Task();
    }
}
