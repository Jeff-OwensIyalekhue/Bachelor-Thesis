using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour {

    #region Vars
    [Header("UI")]
    public TMP_Text timer;
    public TMP_Text numberAText;
    public TMP_Text opText;
    public TMP_Text numberBText;
    public TMP_InputField inputField;
    EventSystem eventSystem;
    public TMP_Text score;
    public TMP_Text scoreBoard;
    List<int> scoreList = new List<int>();
    bool positionInScoreboardSet = false;
    public TMP_Text scoreNotification;
    public Animator scoreNotificationAnim;

    [Header("Task Transitions")]
    public Animator anim;
    public AnimationClip[] entryClip;
    public AnimationClip[] exitClip;
    bool transition = false;

    [Header("Settings")]
    public int intervalMin = 0;
    public int intervalMax = 10;    // exclusive
    public bool showScore = true;

    [Header("")]
    public bool testing = false;

    int numberA; 
    int numberB;
    int op;                         // operator: 0 := addition; 1 := subtraction; 2 := multiplication; else := error
    string opClean;

    int expResult;                  // expected result = numberA op numberB

    int entResult;                  // entered rusult

    float timeTask;                 // start time of a task

    bool answered = true;
    float timeStart;                // start time of the  game after the start countdown
    #endregion

    // Use this for initialization
    void Awake () {
        int r = Random.Range(0, 100);
        GameManager.Instance.participant = new ParticipantData(r, GameManager.Instance.gameMode);
        eventSystem = FindObjectOfType<EventSystem>();
    }
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.preGameRunning)
        {
            StartCoroutine(StartCountdown());
            return;
        }

        if (!GameManager.Instance.gameRunning)
            return;

        // if there is no time limit, end the game when escape is pressed
        if(/*timeLimit <= 0 &&*/ Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.gameRunning = false;

            // Enter participant information
            GameManager.Instance.participant.correctAnswers = GameManager.Instance.correctAnswers;
            GameManager.Instance.participant.wrongAnswers = GameManager.Instance.wrongAnswers;
            GameManager.Instance.participant.skippedTasks = GameManager.Instance.skippedAnswers;
            GameManager.Instance.participant.timePlayed = Time.time - timeStart;

            GameManager.Instance.CreateUserData();

            // Reset GameManager Data
            GameManager.Instance.correctAnswers = 0;
            GameManager.Instance.wrongAnswers = 0;
            GameManager.Instance.skippedAnswers = 0;
            //GameManager.Instance.enemyScore = 0;

            NetworkSync.Load();
        }
        if (GameManager.Instance.gameRunning || testing)
        {
            // if time limit is reached ends the game and returns to menu
            if (GameManager.Instance.timeLimit > 0 && (GameManager.Instance.timeLimit + timeStart - Time.time) <= 0)
            {
                GameManager.Instance.gameRunning = false;
                inputField.DeactivateInputField();
                timer.text = "end";

                // Enter participant information
                GameManager.Instance.participant.correctAnswers = GameManager.Instance.correctAnswers;
                GameManager.Instance.participant.wrongAnswers = GameManager.Instance.wrongAnswers;
                GameManager.Instance.participant.skippedTasks = GameManager.Instance.skippedAnswers;
                GameManager.Instance.participant.timePlayed = Time.time - timeStart;
                GameManager.Instance.CreateUserData();

                // Reset GameManager Data
                GameManager.Instance.correctAnswers = 0;
                GameManager.Instance.wrongAnswers = 0;
                GameManager.Instance.skippedAnswers = 0;
                //GameManager.Instance.enemyScore = 0;

                NetworkSync.Load();
            }

            // display the remaining time if a time limit is set
            if (GameManager.Instance.timeLimit > 0 && GameManager.Instance.showTimer && (GameManager.Instance.timeLimit + timeStart - Time.time) >= 0)
                timer.text = (GameManager.Instance.timeLimit + timeStart - Time.time).ToString("N0");

        // checks if new task should be asked
            if (answered)
            {
                Task();
            }
            
            ScoreUpdate();

            // ensures that the inputfield is selected by the event system
            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(inputField.gameObject);
            }
        }
    }

    IEnumerator StartCountdown()
    {
        GameManager.Instance.preGameRunning = false;
        timer.text = "<color=red>3</color>";
        yield return new WaitForSeconds(1);
        timer.text = "<color=yellow>2</color>";
        yield return new WaitForSeconds(1);
        timer.text = "<color=green>1</color>";
        yield return new WaitForSeconds(1);

        // if timeLimit is set is set to a "invalid time" set, show
        if (GameManager.Instance.timeLimit <= 0)
            timer.text = "no time limit";


        if (!GameManager.Instance.showTimer)
            timer.gameObject.SetActive(false);
        timeStart = Time.time;

        if (!showScore)
            score.gameObject.SetActive(false);

        GameManager.Instance.gameRunning = true;

    }

    void ScoreUpdate()
    {
        if(GameManager.Instance.gameMode < 2)
        {
            score.text = (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString();
        }
        else if (GameManager.Instance.gameMode == 2)
        {
            if (GameManager.Instance.playerList.Count < 2)
                return;

            if (GameManager.Instance.ownConnectionID == 0)
                score.text = "<color=green>" + (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString() + "</color> vs. <color=red>" + GameManager.Instance.playerList[1].score;
            else if (GameManager.Instance.ownConnectionID == 1)
                score.text = "<color=green>" + (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString() + "</color> vs. <color=red>" + GameManager.Instance.playerList[0].score;
        }
        else if(GameManager.Instance.gameMode == 3)
        {
            score.text = (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString();

            //GameManager.Instance.playerList.Sort();
            scoreBoard.text = "";
            scoreList.Clear();
            foreach (NetworkObject nO in GameManager.Instance.playerList)
            {
                scoreList.Add(nO.score);
            }
            scoreList.Sort();
            scoreBoard.text = "Scores:\n";
            for (int i = scoreList.Count - 1; i >= 0; i--)
            {
                if(!positionInScoreboardSet && scoreList[i] == (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers))
                {
                    positionInScoreboardSet = true;
                    scoreBoard.text += "you\n";
                }
                else
                    scoreBoard.text += scoreList[i] + "\n";
            }
            positionInScoreboardSet = false;
        }
    }

    // Sets a new task up
    void Task()
    {
        transition = true;

        answered = false;
        numberA = Random.Range(intervalMin, intervalMax);
        numberB = Random.Range(intervalMin, intervalMax);
        op = Random.Range(0, 3);
        //Debug.Break();
        StartCoroutine(TaskEntry());
    }
    IEnumerator TaskEntry()
    {
        numberAText.text = numberA.ToString();
        numberBText.text = numberB.ToString();
        switch (op)
        {
            case 0:
                expResult = numberA + numberB;
                opText.text = " <color=green> + </color> ";
                opClean = " + ";
                break;
            case 1:
                expResult = numberA - numberB;
                opText.text = " <color=red> - </color> ";
                opClean = " - ";
                break;
            case 2:
                expResult = numberA * numberB;
                opText.text = " <color=#00aaff> * </color> ";
                opClean = " * ";
                break;
            default:
                Debug.Log("unexpected value as operaor");
                break;
        }

        int r = Random.Range(0, entryClip.Length);

        anim.SetTrigger(entryClip[r].name);
        yield return new WaitForSeconds(entryClip[r].length / 2);

        if (!numberAText.gameObject.activeSelf)
            numberAText.gameObject.SetActive(true);
        if (!numberBText.gameObject.activeSelf)
            numberBText.gameObject.SetActive(true);
        if (!opText.gameObject.activeSelf)
            opText.gameObject.SetActive(true);

        timeTask = Time.time;

        transition = false;
    }

    public void Solve()
    {
        if (!testing)
            if (!GameManager.Instance.gameRunning)
                return;
        if (transition)
            return;

        transition = true;

        List<int> eIDs = new List<int>();
        List<int> eScores = new List<int>();
        foreach (NetworkObject nO in GameManager.Instance.playerList)
        {
            if (nO.connectionID != GameManager.Instance.ownConnectionID)
            {
                eIDs.Add(nO.connectionID);
                eScores.Add(nO.score);
            }
        }
        if (GameManager.Instance.gameMode == 0)
        {
            eScores.Clear();
            eScores.Add(0);
            eIDs.Clear();
            eIDs.Add(0);
        }


        if (inputField.text.Length != 0)   
        {
            entResult = int.Parse(inputField.text);
            if (entResult == expResult)
            {
                GameManager.Instance.correctAnswers++;
                scoreNotification.text = "<color=green>+1";
                GameManager.Instance.participant.tasks.Add(new TaskData(Time.time - timeTask, numberAText.text + opClean + numberBText.text + " = " + entResult, true, true, eIDs, eScores));
            }
            else
            {
                GameManager.Instance.wrongAnswers++;
                scoreNotification.text = "<color=red>-1";
                GameManager.Instance.participant.tasks.Add(new TaskData(Time.time - timeTask, numberAText.text + opClean + numberBText.text + " = " + entResult, true, false, eIDs, eScores));
            }
        }
        else
        {
            GameManager.Instance.skippedAnswers++;
            scoreNotification.text = "+0";
            GameManager.Instance.participant.tasks.Add(new TaskData(Time.time - timeTask, numberAText.text + opClean + numberBText.text + " = N.A.", false, false, eIDs, eScores));
        }

        scoreNotificationAnim.SetTrigger("Up");

        StartCoroutine(TaskExit());
    }
    IEnumerator TaskExit()
    {
        int r = Random.Range(0, exitClip.Length);
        anim.SetTrigger(exitClip[r].name);
        yield return new WaitForSeconds(exitClip[r].length);

        numberAText.gameObject.SetActive(false);
        numberBText.gameObject.SetActive(false);
        opText.gameObject.SetActive(false);

        inputField.DeactivateInputField();
        inputField.text = "";
        inputField.ActivateInputField();
        answered = true;
        transition = false;
    }
}
