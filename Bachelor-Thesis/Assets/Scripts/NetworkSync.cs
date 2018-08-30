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

    NetworkObject[] nO;
    NetworkManager networkManager;

    bool startTransition = false;
    static bool endTransition = false;

    public Canvas canvas;
    public Animator anim;
    public AnimationClip clip;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
        nO = FindObjectsOfType<NetworkObject>();
        networkManager = FindObjectOfType<NetworkManager>();
	}

    // Update is called once per frame
    void Update () {
        if (!startTransition)
        {
            if (nO.Length != networkManager.numPlayers)
                nO = FindObjectsOfType<NetworkObject>();

            if (GameManager.Instance.gmClientReady)
            {
                Debug.Log("Looking if everybody is ready.");
                for (int i = 0; i < nO.Length; i++)
                {
                    if (!nO[i].clientReady)
                        return;
                }
                StartCoroutine(StartTransition());
            }
        }

        if (endTransition)
            StartCoroutine(EndTransition());
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
