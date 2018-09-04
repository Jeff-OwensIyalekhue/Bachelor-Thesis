using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMEditorMonitor : MonoBehaviour {

    [SerializeField]
    List<NetworkObject> playerList = GameManager.Instance.playerList;
    public int ownConnectionID = GameManager.Instance.ownConnectionID;
    public int playerListLength = GameManager.Instance.playerList.Count;
  
    #region Singleton
    public static GMEditorMonitor Instance;


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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            playerList = GameManager.Instance.playerList;
            ownConnectionID = GameManager.Instance.ownConnectionID;
            playerListLength = GameManager.Instance.playerList.Count;
        }
    }
}
