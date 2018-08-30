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

    public bool isConnected = false;

    public bool startPressed = false;
    public bool gmClientReady = false;
    public bool everybodyReady = false;
    public int amountPlayerReady = 0;

    public bool preGameRunning = false;
    public bool gameRunning = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    //public void CreateUserData(string s)
    //{

    //    string pathFolder = @"Desktop\TestFolder";

    //    try
    //    {
    //        // Determine whether the directory exists.
    //        if (Directory.Exists(pathFolder))
    //        {
    //            Debug.Log("That path exists already.");
    //            return;
    //        }

    //        // Try to create the directory.
    //        DirectoryInfo di = Directory.CreateDirectory(pathFolder);
    //        Debug.Log("The directory was created successfully at " + Directory.GetCreationTime(pathFolder));

    //        // Delete the directory.
    //        //di.Delete();
    //        //Debug.Log("The directory was deleted successfully.");
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("The process failed: " + e.ToString());
    //    }
    //    finally { }


    //    Debug.Log("create user data");
    //    string path = Application.persistentDataPath + "/" + s + ".txt";
    //    //FileStream file = File.Create(path);
    //    string[] lines = {"UserB","Correct: "+ correctAnswers, "Wrong: " + wrongAnswers, "Appendix"};
    //    File.WriteAllLines(path,lines);
    //    StreamReader sr = new StreamReader(path);
    //    string line = sr.ReadToEnd();
    //    Debug.Log(line);

    //    //file.Close();
    //}

    public void SaveData()
    {
        //BinaryFormatter bF = new BinaryFormatter();
        //FileStream file = File.Create(Application.persistentDataPath + "/testData.txt");

        //SaveData data = new SaveData();
        //data.data = input;

        //bF.Serialize(file, data);
        //file.Close();
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

//class ParticipantData
//{
//    public int id;
//    public int solvedTasks;
//    public int skippedTasks;
//    public int correctAnswers;
//    public int wrongAnswers;
//}
//class TaskData
//{
//    public int ParticipantID;
//    public int id;
//    public float timeNeeded;
//    public string task;
//    public bool solved;
//    public bool correctAnswer;
//}
