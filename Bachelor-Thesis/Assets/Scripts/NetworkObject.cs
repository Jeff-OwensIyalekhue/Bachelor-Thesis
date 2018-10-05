using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    [Header("Autonomous Player")]
    public bool fakePlayer = false;
    public bool scriptedPlayer = false;
    bool scriptRunning = false;
    float[] singleplayerScript = {2.332f, 3.016f, 3.308f, 2.642f, 2.226f, 3.438f, 2.61f, 3.794f, 2.402f, 2.796f, 2.882f,
                                      2.224f, 2.188f, 2.548f, 2.582f, 1.986f, 2.788f, 2.84f, 2.294f, 1.698f, 2.288f, 1.946f,
                                      3.182f, 2.734f, 2.386f, 2.294f, 1.953333333f, 2.026666667f, 1.67f, 2.295f, 4.03f, 1.58f};

    float[] halbcoopScript = {1.802f, 4.12f, 2.272f, 2.44f, 2.656f, 4.874f, 2.334f, 2.482f, 1.868f, 2.228f, 2.578f, 3.174f,
                                2.31f, 2.47f, 2.532f, 2.546f, 2.058f, 2.998f, 2.338f, 1.968f, 2.164f, 2.184f, 2.084f, 2.622f,
                                2.892f, 2.364f, 2.262f, 2.17f, 1.776666667f, 1.87f, 2.015f};

    float[] versusScript = {1.902f,2.18f,2.282f,2.546f,2.186f,3.222f,1.818f,2.576f,2.222f,2.242f,2.074f,
                            2.378f,2.188f,2.21f,2.16f,1.748f,2.24f,2.276f,2.144f,2.116f,2.556f,2.566f,2.1f,
                            2.248f,3.73f,2.616f,2.29f,3.014f,2.12f,2.9f,2.416666667f,1.915f};

    float[] partyScript = {2.794f,2.776f,2.604f,2.566f,2.768f,4.282f,2.8f,3.186f,2.43f,2.85f,
                            2.596f,2.336f,2.23f,2.042f,2.222f,2.064f,2.43f,2.738f,1.71f,2.42f,
                            2.42f,3.124f,2.596f,2.466f,2.624f,2.6f,2.325f,2.7325f,1.83f,1.81f};

    [Header("Status Vars")]
    [SyncVar]
    public int score = 0;

    [Header("Network Info")]
    NetworkManager networkManager;
    [SyncVar]
    public int connectionID = -1;
    [SyncVar]
    public bool clientReady = false;

    [SerializeField]
    [SyncVar]
    float timeLimit;

    [SyncVar]
    public int gameMode = 0;
    

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);

        networkManager = FindObjectOfType<NetworkManager>();

        timeLimit = GameManager.Instance.timeLimit;

        if (fakePlayer)
        {
            connectionID = GameManager.Instance.playerList.Count;
            this.gameObject.name = "Player " + connectionID;
            GameManager.Instance.playerList.Add(this);
            //GameManager.Instance.playerListLength++;
            StartCoroutine(FakePlayerBehavior());
        }
        else
        {
            connectionID += networkManager.numPlayers;
            this.gameObject.name = "Player " + (networkManager.numPlayers - 1);
            GameManager.Instance.playerList.Add(this);
            //GameManager.Instance.playerListLength++;

            if (isLocalPlayer)
            {
                GameManager.Instance.ownConnectionID = (networkManager.numPlayers - 1);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {

        if(!GameManager.Instance.playerList.Contains(this))
            GameManager.Instance.playerList.Add(this);

        if (GameManager.Instance.isHost)
        {
            if (timeLimit != GameManager.Instance.timeLimit)
                timeLimit = GameManager.Instance.timeLimit;
        }
        else
        {
            if (timeLimit != GameManager.Instance.timeLimit)
                GameManager.Instance.timeLimit = timeLimit;
        }

        if (isLocalPlayer)
        {

            if(connectionID != GameManager.Instance.ownConnectionID)
            {
                GameManager.Instance.ownConnectionID = connectionID;
                 this.gameObject.name = "Player " + connectionID;
            }

            if (GameManager.Instance.gameRunning)
            {
                if (!scriptRunning && scriptedPlayer)
                    StartCoroutine(ScriptedPlayerBehavior());

                if (this.score != GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers || (fakePlayer && score != GameManager.Instance.playerList[connectionID].score))
                {
                    CmdScoreUpdate(GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers);
                }
            }
            else
            {
                if(GameManager.Instance.ownConnectionID != 0)
                    if (GameManager.Instance.gameMode != GameManager.Instance.playerList[0].gameMode)
                        GameManager.Instance.gameMode = GameManager.Instance.playerList[0].gameMode;

                if (gameMode == GameManager.Instance.playerList[0].gameMode)
                {
                    if (gameMode != GameManager.Instance.gameMode)
                        CmdModeUpdate(GameManager.Instance.gameMode);
                }
                //else
                //{
                //    GameManager.Instance.gameMode = GameManager.Instance.playerList[0].gameMode;
                //    if (gameMode != GameManager.Instance.gameMode)
                //        CmdModeUpdate(GameManager.Instance.gameMode);
                //}
            }

            // set client unready if start is not pressed
            if (!GameManager.Instance.startPressed && clientReady)
            {
                CmdSetReady();
                GameManager.Instance.gmClientReady = false;
            }
            // set client ready if start is pressed
            if (GameManager.Instance.startPressed && !GameManager.Instance.gmClientReady)
            {
                if (!clientReady)
                {
                    CmdSetReady();
                    GameManager.Instance.gmClientReady = true;
                }
            }

            if(GameManager.Instance.everybodyReady)
            {
                CmdSetReady();
                GameManager.Instance.everybodyReady = false;
                GameManager.Instance.gmClientReady = false;
                GameManager.Instance.startPressed = false;
                //if (isServer)
                //    networkManager.ServerChangeScene("Game");

            }
        }
        else
        {
            if (!GameManager.Instance.gameRunning && !GameManager.Instance.isHost)
            {
                if (GameManager.Instance.nGameMode != gameMode)
                {
                    GameManager.Instance.nGameMode = GameManager.Instance.gameMode = gameMode;
                }
            }
        }
    }

    IEnumerator FakePlayerBehavior()
    {
        int factor = Random.Range(0, 1);
        float r = 0;
        float boost = 0;
        while (GameManager.Instance.gameRunning)
        {
            r = Random.Range(1.11f, 2.49f + (2.49f - 1.11f));
            yield return new WaitForSeconds(r);

            r = Random.Range(0f, 1f);
            if (score < GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers)
            {
                if (score + 3 + factor < GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers)
                    boost = 0.04f;
                else
                    boost = 0;

                if (r <= 0.7 + (boost * 4))
                {
                    score++;
                }
                else if (r > 0.95 + boost)
                {
                    score--;
                }
            }
            else
            {
                if (score - 3 - factor >= GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers)
                    boost = 0.04f;
                else
                    boost = 0;

                if (r <= 0.5 - boost)
                {
                    score++;
                }
                else if (r > 0.9 - boost)
                {
                    score--;
                }
            }
        }
        score = 0;
    }

    IEnumerator ScriptedPlayerBehavior()
    {
        scriptRunning = true;
        while (GameManager.Instance.gameRunning)
        {
            switch (gameMode)
            {
                case 0:
                    foreach (float time in singleplayerScript)
                    {
                        yield return new WaitWhile(() => GameLogic.transition);
                        //Debug.Log("task readable");
                        yield return new WaitForSeconds(1.5f);
                        GameLogic.SolveCountdown(time + Time.time);
                        yield return new WaitForSeconds(time);
                        if (GameManager.Instance.supervisor)
                        {
                            GameLogic.ScriptSolve();
                        }
                        else
                        {
                            score++;
                        }
                    }
                    break;
                case 1:
                    foreach (float time in halbcoopScript)
                    {
                        yield return new WaitWhile(() => GameLogic.transition);
                        yield return new WaitForSeconds(1.5f);
                        GameLogic.SolveCountdown(time + Time.time);
                        yield return new WaitForSeconds(time);
                        if (GameManager.Instance.supervisor)
                        {
                            GameLogic.ScriptSolve();
                        }
                        else
                        {
                            score++;
                        }
                    }
                    break;
                case 2:
                    foreach (float time in versusScript)
                    {
                        yield return new WaitWhile(() => GameLogic.transition);
                        yield return new WaitForSeconds(1.5f);
                        GameLogic.SolveCountdown(time + Time.time);
                        yield return new WaitForSeconds(time);
                        if (GameManager.Instance.supervisor)
                        {
                            GameLogic.ScriptSolve();
                        }
                        else
                        {
                            score++;
                        }
                    }
                    break;
                case 3:
                    foreach (float time in partyScript)
                    {
                        yield return new WaitWhile(() => GameLogic.transition);
                        yield return new WaitForSeconds(1.5f);
                        GameLogic.SolveCountdown(time + Time.time);
                        yield return new WaitForSeconds(time);
                        if (GameManager.Instance.supervisor)
                        {
                            GameLogic.ScriptSolve();
                        }
                        else
                        {
                            score++;
                        }
                    }
                    break;
                default:
                    foreach (float time in singleplayerScript)
                    {
                        yield return new WaitWhile(() => GameLogic.transition);
                        yield return new WaitForSeconds(1.5f);
                        GameLogic.SolveCountdown(time + Time.time);
                        yield return new WaitForSeconds(time);
                        if (GameManager.Instance.supervisor)
                        {
                            GameLogic.ScriptSolve();
                        }
                        else
                        {
                            score++;
                        }
                    }
                    break;
            }
        }
        scriptRunning = false;
    }

    [Command]
    public void CmdModeUpdate(int i)
    {
        gameMode = i;
    }

    [Command]
    public void CmdScoreUpdate(int i)
    {
        score = i;
    }

    [Command]
    public void CmdSetReady()
    {
        if (clientReady)
        {
            clientReady = false;
            //Debug.Log(gameObject.name + " is not ready");
            return;
        }
        clientReady = true;
        //Debug.Log(gameObject.name + " is ready");
    }
}
