using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    public bool fakePlayer = false;

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
                if (this.score != GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers || fakePlayer)
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
            r = Random.Range(2f, 6f);
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
