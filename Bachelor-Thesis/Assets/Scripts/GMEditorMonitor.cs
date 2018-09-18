using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMEditorMonitor : MonoBehaviour {

    [SerializeField]
    List<NetworkObject> playerList = GameManager.Instance.playerList;
    public string path = GameManager.Instance.pathToSaveLocation;
    public int turn = GameManager.Instance.currentParticipantTurn;
  
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
            turn = GameManager.Instance.currentParticipantTurn;
            //playerList = GameManager.Instance.playerList;
            //path= GameManager.Instance.pathToSaveLocation;
        }
    }
}
