using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager{

    #region Instanziierung
    private static GameManager instance;

    private GameManager()
    {
        if (instance != null)
            return;
        instance = this;

    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }

            return instance;
        }
    }
    #endregion

    public int correctAnswers = 0;
    public int wrongAnswers = 0;
    public int skippedAnswers = 0;

    public bool gameRunning = false;

    public string input = "test";

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SaveData()
    {
        BinaryFormatter bF = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/testData.txt");

        SaveData data = new SaveData();
        data.data = input;
        
        bF.Serialize(file, data);
        file.Close();
    }

    public void LoadData()
    {

    }

    // End the game / return to the desktop
    public void EndGame()
    {
        //If we are running in a standalone build of the game
        #if UNITY_STANDALONE
            //Quit the application
            Application.Quit();
        #endif

        //If we are running in the editor
        #if UNITY_EDITOR
            //Stop playing the scene
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

[Serializable]
class SaveData
{
    public string data;
}
