﻿using System.Collections;
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

    public bool showTimer = true;
    public float timeLimit = 0;            // if value <= 0: no time limit
    public string pathToSaveLocation = "I:/";
    public float musicVolume = -80;

    public ParticipantData participant;

    public int correctAnswers = 0;
    public int wrongAnswers = 0;
    public int skippedAnswers = 0;

    public int gameMode = 0;            // 0:= SinglePlayer; 1:= HalbCoop; 2:= Versus
    public int nGameMode = -1;
    public int ownConnectionID = 0;

    public List<NetworkObject> playerList = new List<NetworkObject>();
    
    //public int playerListLength = 0;

    public bool preGameRunning = false;
    public bool gameRunning = false;
    
    public bool isConnected = false;
    public bool isHost = false;

    public bool startPressed = false;
    public bool gmClientReady = false;
    public bool everybodyReady = false;
    
    public void CreateUserData()
    {

        string pathFolder = pathToSaveLocation + "Particpant" + participant.identification;

        try
        {
            // Determine whether the directory exists.
            while (Directory.Exists(pathFolder))
            {
                participant.identification++;
                pathFolder = pathToSaveLocation + "Particpant" + participant.identification; ;
                //return;
            }

            // Try to create the directory.
            Directory.CreateDirectory(pathFolder);
            Debug.Log("The directory was created successfully at " + Directory.GetCreationTime(pathFolder));

            // Delete the directory.
            //di.Delete();
            //Debug.Log("The directory was deleted successfully.");
        }
        catch (Exception e)
        {
            Debug.Log("The process failed: " + e.ToString());
        }
        finally { }

        // Creating a human readable .txt file of particpant data
        //Debug.Log("create user data");
        string path = pathFolder + "/Particpant" + participant.identification + "_HR.txt";
        
        using(StreamWriter file = new StreamWriter(path, true))
        {
            file.WriteLine(DateTime.Today.ToString("D") + ", " + DateTime.Now.ToString("h:mm tt"));

            file.WriteLine("Particpant " + participant.identification);
            file.WriteLine("Player " + participant.connectionID);

            switch (participant.gameMode)
            {
                case 0:
                    file.WriteLine("Singleplayer");
                    break;
                case 1:
                    file.WriteLine("Halb Coop");
                    break;
                case 2:
                    file.WriteLine("Versus");
                    break;
                case 3:
                    file.WriteLine("Party");
                    break;
                default:
                    file.WriteLine("Unknown Mode");
                    break;
            }

            file.WriteLine(participant.timePlayed.ToString("N2") + " Seconds");

            file.WriteLine("Tasks:");
            file.WriteLine("    Overall: " + (participant.correctAnswers + participant.wrongAnswers + participant.skippedTasks).ToString());
            file.WriteLine("        Solved: " + (participant.correctAnswers + participant.wrongAnswers).ToString());
            file.WriteLine("            Correct Solutions: " + (participant.correctAnswers).ToString());
            file.WriteLine("            Wrong Solutions: " + (participant.wrongAnswers).ToString());
            file.WriteLine("        Skipped: " + participant.skippedTasks);

            file.WriteLine("TaskData:");
            int i = 1;
            foreach(TaskData task in participant.tasks)
            {
                file.WriteLine(i + ". Task **************************");
                i++;
                file.WriteLine(task.task);
                file.WriteLine("time needed in seconds: " + task.timeNeeded.ToString("N2"));
                file.WriteLine("solved: " + task.solved);
                file.WriteLine("correct answer: " + task.correctAnswer);
                file.WriteLine("Enemy score at this point: ");
                for(int j = 0; j < task.enemyIDs.Count; j++)
                {
                    file.WriteLine("    Player " + task.enemyIDs[j] + " : " + task.enemyScore[j]);
                }
                file.WriteLine("**********************************");
            }

        }

        // Creating a serialized file of particpant data
        BinaryFormatter bF = new BinaryFormatter();
        FileStream fileB = File.Create(pathFolder + "/Particpant" + participant.identification + "_NHR.dat");

        ParticipantData data = participant;

        bF.Serialize(fileB, data);
        fileB.Close();

    }

    public void SaveOptions()
    {
        BinaryFormatter bF = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath +"/options.dat");

        OptionsData data = new OptionsData(showTimer, timeLimit, pathToSaveLocation, musicVolume);
        
        bF.Serialize(file, data);
        file.Close();
    }

    public void LoadOptions()
    {
        if (File.Exists(Application.persistentDataPath + "/options.dat"))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/options.dat", FileMode.Open);
            OptionsData data = (OptionsData)bF.Deserialize(file);
            file.Close();

            showTimer = data.showTimer;
            timeLimit = data.timeLimit;
            pathToSaveLocation = data.pathToSaveLocation;
            musicVolume = data.musicVolume;
        }
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
public class ParticipantData
{
    public int identification;
    public int connectionID;
    public int gameMode;
    public float timePlayed;
    public int skippedTasks;
    public int correctAnswers;
    public int wrongAnswers;
    public List<TaskData> tasks;

    public ParticipantData(int id, int gM)
    {
        identification = id;
        connectionID = GameManager.Instance.ownConnectionID;
        gameMode = gM;
        timePlayed = 0;
        skippedTasks = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        tasks = new List<TaskData>();
    }
}
[Serializable]
public class TaskData
{
    public float timeNeeded;
    public string task;
    public bool solved;
    public bool correctAnswer;
    public List<int> enemyIDs;
    public List<int> enemyScore;

    public TaskData(float t, string ta, bool s, bool c, List<int> eID,List<int> eS)
    {
        timeNeeded = t;
        task = ta;
        solved = s;
        correctAnswer = c;
        enemyIDs = eID;
        enemyScore = eS;
    }
}

[Serializable]
public class OptionsData
{
    public bool showTimer;
    public float timeLimit;
    public string pathToSaveLocation;
    public float musicVolume;

    public OptionsData(bool sT, float t, string p, float m)
    {
        showTimer = sT;
        timeLimit = t;
        pathToSaveLocation = p;
        musicVolume = m;
    }
}

