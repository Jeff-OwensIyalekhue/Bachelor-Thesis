using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSync : NetworkBehaviour {
    
    #region Singleton
    public static NetworkSync Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(transform.gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    NetworkManager networkManager;
    [Header("FakePlayer")]
    bool fakeOpponent = false;
    public GameObject fakeEnemyPrefab;
    public int fakeEnemyAmount = 3;

    [Header("Scene Blackscreen Fade")]
    public Canvas canvas;
    public Animator anim;
    public AnimationClip clip;

    bool startTransition = false;
    static bool endTransition = false;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
        networkManager = FindObjectOfType<NetworkManager>();
    }

    // Update is called once per frame
    void Update () {        
        if (GameManager.Instance.playerList.Count == 1)
        {
            if(GameManager.Instance.gameRunning && (GameManager.Instance.gameMode == 2 || GameManager.Instance.gameMode == 3) && GameManager.Instance.isHost)
            {
                if (!fakeOpponent)
                {
                    fakeOpponent = true;
                    StartCoroutine(FakePlayer(1));
                }
            }
        }
        if (GameManager.Instance.playerList.Count <= fakeEnemyAmount && GameManager.Instance.isHost)
        {
            if (GameManager.Instance.gameRunning && GameManager.Instance.gameMode == 4)
            {
                if (!fakeOpponent)
                {
                    fakeOpponent = true;
                    StartCoroutine(FakePlayer(1 + fakeEnemyAmount - GameManager.Instance.playerList.Count));
                }
            }
        }
        if (!GameManager.Instance.gameRunning && GameManager.Instance.playerList.Count > networkManager.numPlayers)
        {
            for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
            {
                if (GameManager.Instance.playerList[i].Equals(null) /*|| GameManager.Instance.playerList[i].fakePlayer*/)
                {
                    GameManager.Instance.playerList.RemoveAt(i);
                    //GameManager.Instance.playerListLength--;
                }
            }
        }

        // Checking if game can start
        if (!startTransition)
        {
            if (GameManager.Instance.gmClientReady)
            {
                if(GameManager.Instance.gameMode == 0)
                {
                    StartCoroutine(StartTransition());
                }
                else if((GameManager.Instance.gameMode == 2 || GameManager.Instance.gameMode == 3) && GameManager.Instance.playerList.Count >= 2)
                {
                    if (GameManager.Instance.playerList[0].clientReady)
                        if (GameManager.Instance.playerList[1].clientReady)
                            StartCoroutine(StartTransition());
                }
                else
                {
                    foreach (NetworkObject nO in GameManager.Instance.playerList)
                    {
                        if (!nO.clientReady)
                            return;
                    }
                    StartCoroutine(StartTransition());
                }
            }
        }

        if (endTransition)
        {
            endTransition = false;
            StartCoroutine(EndTransition());
        }
    }

    IEnumerator FakePlayer(int i)
    {
        // add i  time create fplayer
        Debug.Log("Fake opponent");
        GameObject[] dummy = new GameObject[i];
        //NetworkObject[] nO = new NetworkObject[i];
        for(int j = 0; j  < i; j++)
        {
            dummy[j] = Instantiate(fakeEnemyPrefab);
            NetworkServer.Spawn(dummy[j]);
        }
        dummy[0].GetComponent<NetworkObject>().scriptedPlayer = true;

        yield return new WaitWhile(() => GameManager.Instance.gameRunning);

        for (int j = 0; j < i; j++)
        {
            Destroy(dummy[i - 1 - j]);
        }

        fakeOpponent = false;
    }

    public static void Load()
    {
        endTransition = true;
    }
    IEnumerator EndTransition()
    {
        yield return new WaitForSeconds(3);

        // Reset GameManager Data
        GameManager.Instance.correctAnswers = 0;
        GameManager.Instance.wrongAnswers = 0;
        GameManager.Instance.skippedAnswers = 0;
        //GameManager.Instance.enemyScore = 0;

        AsyncOperation async;
        //Debug.Log("begin transition");
        //Debug.Break();
        endTransition = false;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds((clip.length / 2) + 1);
        //Debug.Log("before scene change");
        //Debug.Break();
        async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        //if (isServer)
        //    networkManager.ServerChangeScene("Menu");
        yield return new WaitUntil(() => async.isDone);
        anim.SetTrigger("End");
        yield return new WaitUntil(() => anim.IsInTransition(0));
        canvas.sortingOrder = -1;
    }
    IEnumerator StartTransition()
    {
        //Debug.Break();
        AsyncOperation async;
        startTransition = true;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds((clip.length / 2) + 1);
        GameManager.Instance.everybodyReady = true;
        async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
        yield return new WaitUntil(() => async.isDone);
        anim.SetTrigger("End");
        yield return new WaitUntil(() => anim.IsInTransition(0));
        GameManager.Instance.preGameRunning = true;
        //yield return new WaitForSeconds((clip.length / 2) - 1);
        canvas.sortingOrder = -1;
        startTransition = false;
    }
}
