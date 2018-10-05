using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour {

    #region Vars
    [Header("General UI")]
    public TMP_Text timer;
    public TMP_Text numberAText;
    public TMP_Text opText;
    public TMP_Text numberBText;
    public TMP_InputField inputField;
    EventSystem eventSystem;
    public TMP_Text inputFieldPlaceholder;
    public TMP_Text supervisorNotification;
    static float autoSolveCountdown;


    [Header("Score UI")]
    public TMP_Text score;
    public TMP_Text scoreBoard;
    List<int> scoreList = new List<int>();
    bool positionInScoreboardSet = false;
    public TMP_Text scoreNotification;
    public Animator scoreNotificationAnim;
    public Animator scoreBoardAnim;
    public AudioSource audioSource;
    public AudioClip positive;
    public AudioClip negativ;
    public AudioClip drop;

    [Header("Task Transitions")]
    public Animator anim;
    public AnimationClip[] entryClip;
    public AnimationClip[] exitClip;
    public static bool transition = false;

    #region supervisor
    string solString;
    char[] solChar;
    int solIter = 0;
    #endregion

    int ranking = 0;
    int taskIteration = 0;

    int numberA; 
    int numberB;
    int op;                         // operator: 0 := addition; 1 := subtraction; 2 := multiplication; else := error
    string opClean;

    int expResult;                  // expected result = numberA op numberB

    int entResult;                  // entered rusult

    float startTime, timeTask;      // start time of a task

    bool answered = true;
    float timeStart;                // start time of the  game after the start countdown
    #endregion

    static bool scriptedSolution;

    // Use this for initialization
    void Awake () {
        //int r = Random.Range(0, 100);
        GameManager.Instance.ownParticipant = new ParticipantData(GameManager.Instance.gameMode, System.DateTime.Now);
        eventSystem = FindObjectOfType<EventSystem>();
        if (!GameManager.Instance.supervisor)
            inputFieldPlaceholder.text = "";
        else
        {
            inputField.interactable = false;
        }
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
        if(Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.gameRunning)
        {
            GameManager.Instance.gameRunning = false;
            inputField.gameObject.SetActive(false);
            numberAText.gameObject.SetActive(false);
            numberBText.gameObject.SetActive(false);
            opText.text = "<size=+100>end";
            if (!GameManager.Instance.showTimer)
                timer.gameObject.SetActive(true);

            // Enter participant information
            GameManager.Instance.ownParticipant.correctAnswers = GameManager.Instance.correctAnswers;
            GameManager.Instance.ownParticipant.wrongAnswers = GameManager.Instance.wrongAnswers;
            GameManager.Instance.ownParticipant.skippedTasks = GameManager.Instance.skippedAnswers;
            GameManager.Instance.ownParticipant.timePlayed = Time.time - timeStart;

            GameManager.Instance.CreateUserData(GameManager.Instance.ownParticipant);
            
            NetworkSync.Load();
        }
        if (GameManager.Instance.gameRunning)
        {
            // if time limit is reached ends the game and returns to menu
            if (GameManager.Instance.timeLimit > 0 && (GameManager.Instance.timeLimit + timeStart - Time.time) <= 0)
            {
                GameManager.Instance.gameRunning = false;
                inputField.gameObject.SetActive(false);
                numberAText.gameObject.SetActive(false);
                numberBText.gameObject.SetActive(false);
                opText.text = "<size=+100>end";
                if (!GameManager.Instance.showTimer)
                    timer.gameObject.SetActive(true);

                // Enter participant information
                GameManager.Instance.ownParticipant.correctAnswers = GameManager.Instance.correctAnswers;
                GameManager.Instance.ownParticipant.wrongAnswers = GameManager.Instance.wrongAnswers;
                GameManager.Instance.ownParticipant.skippedTasks = GameManager.Instance.skippedAnswers;
                GameManager.Instance.ownParticipant.timePlayed = Time.time - timeStart;
                GameManager.Instance.CreateUserData(GameManager.Instance.ownParticipant);

                NetworkSync.Load();
            }

            // display the remaining time if a time limit is set
            if (GameManager.Instance.timeLimit > 0 && GameManager.Instance.showTimer && (GameManager.Instance.timeLimit + timeStart - Time.time) >= 0)
                timer.text = (GameManager.Instance.timeLimit + timeStart - Time.time).ToString("N0");

            if (GameManager.Instance.supervisor)
            {
                supervisorNotification.text = (autoSolveCountdown - Time.time).ToString("N0");
                if (Input.anyKeyDown && !scriptedSolution)
                {
                    if(solIter < solChar.Length)
                    {
                        if(inputField.text.Length < solIter + 1)
                            inputField.text += solChar[solIter];
                        solIter++;
                    }
                }
                if (scriptedSolution)
                {
                    scriptedSolution = false;
                    inputField.text = "" + expResult;
                    Solve();
                }

            }

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
        int r = Random.Range(0, entryClip.Length);
        anim.SetTrigger(entryClip[r].name);
        timer.text = "<color=green>1</color>";
        yield return new WaitForSeconds(1);

        // if timeLimit is set is set to a "invalid time" set, show
        if (GameManager.Instance.timeLimit <= 0)
            timer.text = "no time limit";


        if (!GameManager.Instance.showTimer)
            timer.gameObject.SetActive(false);
        timeStart = Time.time;

        if (!GameManager.Instance.showScore)
            score.gameObject.SetActive(false);

        GameManager.Instance.gameRunning = true;
        startTime = Time.time;

        if (GameManager.Instance.currentParticipantTurn >= GameManager.Instance.turnsToPlay)
            GameManager.Instance.currentParticipantTurn = 0;
        GameManager.Instance.currentParticipantTurn++;
    }

    void ScoreUpdate()
    {
        if(GameManager.Instance.gameMode < 2)
        {
            score.text = /*"<size=+50>" + */(GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString();
        }
        else if (GameManager.Instance.gameMode == 2)
        {
            if (GameManager.Instance.playerList.Count < 2)
                return;

            if (GameManager.Instance.ownConnectionID == 0)
                score.text = /*"<size=+50>" + */"<color=green>" + (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString() + "</color> vs. <color=red>" + GameManager.Instance.playerList[1].score;
            else if (GameManager.Instance.ownConnectionID == 1)
                score.text = /*"<size=+50>" + */"<color=green>" + (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString() + "</color> vs. <color=red>" + GameManager.Instance.playerList[0].score;
        }
        else if(GameManager.Instance.gameMode == 3)
        {
            score.text = /*"<size=150>" + */(GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers).ToString();

            //GameManager.Instance.playerList.Sort();
            scoreBoard.text = "";
            scoreList.Clear();
            foreach (NetworkObject nO in GameManager.Instance.playerList)
            {
                scoreList.Add(nO.score);
            }
            scoreList.Sort();
            scoreBoard.text = "<size=+20>" + "Scores:\n";
            for (int i = scoreList.Count - 1; i >= 0; i--)
            {
                if(!positionInScoreboardSet && scoreList[i] == (GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers))
                {
                    if (i <= ranking && i != scoreList.Count - 1)
                        scoreBoardAnim.SetTrigger("notify");
                    positionInScoreboardSet = true;
                    ranking = i;
                    scoreBoard.text += "you\n";
                }
                else
                    scoreBoard.text += scoreList[i] + "\n";
            }
            positionInScoreboardSet = false;
        }
        else if (GameManager.Instance.gameMode == 4)
        {
            int s = 0;
            foreach (NetworkObject nO in GameManager.Instance.playerList)
            {
                s += nO.score;
            }

            score.text = "" +s;
        }
    }

    public static void ScriptSolve()
    {
        scriptedSolution = true;
    }
    public static void SolveCountdown(float f)
    {
        autoSolveCountdown = f;   
    }
    // Sets a new task up
    void Task()
    {
        transition = true;

        answered = false;

        if (GameManager.Instance.randomTasks)
        {
            numberA = Random.Range(GameManager.Instance.intervalMin, GameManager.Instance.intervalMax);
            op = Random.Range(0, 3);
            numberB = Random.Range(GameManager.Instance.intervalMin, GameManager.Instance.intervalMax);
        }
        else
        {
            int[][] turn = GameManager.Instance.generatedTasks.turns[GameManager.Instance.currentParticipantTurn - 1];
            numberA = turn[taskIteration][0];
            op = turn[taskIteration][1];
            numberB = turn[taskIteration][2];
            taskIteration++;
        }

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
                opText.text = "<color=green>+</color>";
                opClean = " + ";
                break;
            case 1:
                expResult = numberA - numberB;
                opText.text = "<color=red>-</color>";
                opClean = " - ";
                break;
            case 2:
                expResult = numberA * numberB;
                opText.text = "<color=#00aaff>*</color>";
                opClean = " * ";
                break;
            default:
                Debug.Log("unexpected value as operator");
                break;
        }
        if (GameManager.Instance.supervisor)
        {
            inputFieldPlaceholder.text = "" + expResult;
            solString = "" + expResult;
            solChar = solString.ToCharArray();
            solIter = 0;
        }
        //int r = Random.Range(0, entryClip.Length);

        //anim.SetTrigger(entryClip[r].name);
        //yield return new WaitForSeconds(entryClip[r].length * (1/3));

        if (!numberAText.gameObject.activeSelf)
            numberAText.gameObject.SetActive(true);
        if (!numberBText.gameObject.activeSelf)
            numberBText.gameObject.SetActive(true);
        if (!opText.gameObject.activeSelf)
            opText.gameObject.SetActive(true);

        timeTask = Time.time;

        transition = false;
        yield return null;
    }

    public void Solve()
    {
        if (!GameManager.Instance.gameRunning)
            return;
        if (transition)
            return;

        transition = true;

        List<int> eIDs = new List<int>();
        List<int> eScores = new List<int>();
        bool behind = false;
        foreach (NetworkObject nO in GameManager.Instance.playerList)
        {
            if (nO.connectionID != GameManager.Instance.ownConnectionID)
            {
                eIDs.Add(nO.connectionID);
                eScores.Add(nO.score);

                if (GameManager.Instance.playerList[GameManager.Instance.ownConnectionID].score < nO.score
                    && !behind)
                    behind = true;
            }
        }
        
        if (inputField.text.Length != 0 && inputField.text != "-")
        {
            entResult = int.Parse(inputField.text);
            if (entResult == expResult)
            {
                audioSource.clip = positive;
                if(GameManager.Instance.showScore)
                    scoreNotificationAnim.SetTrigger("Up");
                GameManager.Instance.correctAnswers++;
                scoreNotification.text = "<size=+20>" + "<color=green>+1";
                GameManager.Instance.ownParticipant.tasks.Add(new TaskData(timeTask - startTime, Time.time - startTime, numberAText.text + opClean + numberBText.text + " = " + entResult, true, true, behind, eIDs, eScores));
            }
            else
            {
                audioSource.clip = negativ;
                if (GameManager.Instance.showScore)
                    scoreNotificationAnim.SetTrigger("Down");
                GameManager.Instance.wrongAnswers++;
                scoreNotification.text = "<size=+20>" + "<color=red>-1";
                GameManager.Instance.ownParticipant.tasks.Add(new TaskData(timeTask - startTime, Time.time - startTime, numberAText.text + opClean + numberBText.text + " = " + entResult, true, false, behind, eIDs, eScores));
            }
        }
        else
        {
            audioSource.clip = drop;
            if (GameManager.Instance.showScore)
                scoreNotificationAnim.SetTrigger("N");
            GameManager.Instance.skippedAnswers++;
            scoreNotification.text = "<size=+20>" + "+0";
            GameManager.Instance.ownParticipant.tasks.Add(new TaskData(timeTask - startTime, Time.time - startTime, numberAText.text + opClean + numberBText.text + " = N.A.", false, false, behind, eIDs, eScores));
        }


        StartCoroutine(TaskExit());
    }
    IEnumerator TaskExit()
    {
        yield return new WaitForSeconds(0.6f);
        if (GameManager.Instance.showScore)
            audioSource.Play();
        int r = Random.Range(0, exitClip.Length);
        anim.SetTrigger(exitClip[r].name);
        yield return new WaitForSeconds(exitClip[r].length);

        //numberAText.gameObject.SetActive(false);
        //numberBText.gameObject.SetActive(false);
        //opText.gameObject.SetActive(false);

        inputField.DeactivateInputField();
        inputField.text = "";
        inputField.ActivateInputField();
        answered = true;
        transition = false;
    }
}
