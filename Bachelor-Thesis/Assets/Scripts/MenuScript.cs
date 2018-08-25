using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {
    
    int sceneToLoad = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartGame()
    {
        
        LoadSceneByID(sceneToLoad);
    }

    public void SetSceneToLoad(int i)
    {
        sceneToLoad = i + 1;
    }

    public void LoadSceneByID(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void LoadSceneByName(string s)
    {
        SceneManager.LoadScene(s);
    }
}
