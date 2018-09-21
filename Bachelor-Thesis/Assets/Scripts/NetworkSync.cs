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

    bool startTransition = false;
    static bool endTransition = false;

    bool fakeOpponent = false;
    public GameObject fakeEnemyPrefab;

    [Header("Scene Blackscreen Fade")]
    public Canvas canvas;
    public Animator anim;
    public AnimationClip clip;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
        networkManager = FindObjectOfType<NetworkManager>();
    }

    // Update is called once per frame
    void Update () {        
        if (GameManager.Instance.playerList.Count == 1)
        {
            if(GameManager.Instance.gameRunning && GameManager.Instance.gameMode == 2 && GameManager.Instance.isHost)
            {
                if (!fakeOpponent)
                {
                    fakeOpponent = true;
                    StartCoroutine(FakePlayer(1));
                }
            }
        }
        if (GameManager.Instance.playerList.Count < 4 && GameManager.Instance.isHost)
        {
            if (GameManager.Instance.gameRunning && GameManager.Instance.gameMode == 3)
            {
                if (!fakeOpponent)
                {
                    fakeOpponent = true;
                    StartCoroutine(FakePlayer(4 - GameManager.Instance.playerList.Count));
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
                else if( GameManager.Instance.gameMode == 2 && GameManager.Instance.playerList.Count >= 2)
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
            StartCoroutine(EndTransition());
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
        //Debug.Log("begin transition");
        //Debug.Break();
        endTransition = false;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds((clip.length / 2) + 1);
        //Debug.Log("before scene change");
        //Debug.Break();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        //if (isServer)
        //    networkManager.ServerChangeScene("Menu");
        yield return new WaitForSeconds((clip.length / 2) - 1);
        canvas.sortingOrder = -1;
    }
    IEnumerator StartTransition()
    {
        startTransition = true;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds((clip.length / 2) + 1);
        GameManager.Instance.everybodyReady = true;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
        GameManager.Instance.preGameRunning = true;
        yield return new WaitForSeconds((clip.length / 2) - 1);
        canvas.sortingOrder = -1;
        startTransition = false;
    }
}
