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

        if(GameManager.Instance.playerListLength == 1)
        {
            if(GameManager.Instance.gameRunning && GameManager.Instance.gameMode == 2)
            {
                if (!fakeOpponent)
                {
                    fakeOpponent = true;
                    StartCoroutine(FakePlayer());
                }
            }
        }

        if(GameManager.Instance.playerListLength > networkManager.numPlayers)
        {
            for(int i = 0; i < GameManager.Instance.playerListLength; i++)
            {
                if(GameManager.Instance.playerList[i] == null)
                {
                    GameManager.Instance.playerList.RemoveAt(i);
                    GameManager.Instance.playerListLength--;
                }
            }
        }

        // Checking if game can start
        if (!startTransition)
        {
            if (GameManager.Instance.gmClientReady)
            {
                //Debug.Log("Looking if everybody is ready.");
                foreach (NetworkObject nO in GameManager.Instance.playerList)
                {
                    if (!nO.clientReady)
                        return;
                }
                StartCoroutine(StartTransition());
            }
        }

        if (endTransition)
            StartCoroutine(EndTransition());
    }

    IEnumerator FakePlayer()
    {
        Debug.Log("Fake opponent");
        GameObject dummy = Instantiate(fakeEnemyPrefab);
        dummy.name = "fakeOpponent";
        NetworkObject nO = fakeEnemyPrefab.GetComponent<NetworkObject>();
        GameManager.Instance.playerList.Add(nO);
        GameManager.Instance.playerListLength++;

        float r = 0;
        while (GameManager.Instance.gameRunning)
        {
            yield return new WaitForSeconds(6);
            r = Random.Range(0f, 1f);
            if (GameManager.Instance.playerList[1].score < GameManager.Instance.correctAnswers - GameManager.Instance.wrongAnswers)
            {
                if (r <= 0.7)
                {
                    GameManager.Instance.playerList[1].score++;
                }
                else if (r > 0.95)
                {
                    GameManager.Instance.playerList[1].score--;
                }
            }
            else
            {
                if (r <= 0.5)
                {
                    GameManager.Instance.playerList[1].score++;
                }
                else if (r > 0.9)
                {
                    GameManager.Instance.playerList[1].score--;
                }
            }
        }
        
        GameManager.Instance.playerList.Remove(nO);
        GameManager.Instance.playerListLength--;
        Destroy(dummy);
        fakeOpponent = false;
    }

    public static void Load()
    {
        endTransition = true;
    }
    IEnumerator EndTransition()
    {
        endTransition = false;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds((clip.length / 2) + 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        //if (isServer)
        //    networkManager.ServerChangeScene("Menu");
        yield return new WaitForSeconds((clip.length / 2)-1);
        canvas.sortingOrder = -1;
    }
    IEnumerator StartTransition()
    {
        startTransition = true;
        canvas.sortingOrder = 2;
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(clip.length / 2);
        GameManager.Instance.everybodyReady = true;
        GameManager.Instance.preGameRunning = true;
        yield return new WaitForSeconds(clip.length / 2);
        canvas.sortingOrder = -1;
        startTransition = false;
    }
}
