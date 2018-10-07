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

    #region setting vars
    public bool supervisor = false;

    public bool randomTasks = false;
    public int taskPerTurn = 200;
    public int turnsToPlay = 4;
    public int intervalMin = 0;
    public int intervalMax = 10;                            // exclusive
    public bool showScore = true;

    public bool showTimer = true;
    public float timeLimit = 0;                             // if value <= 0: no time limit

    public string pathToSaveLocation = "I:/";

    public float musicVolume = -80;
    public float sfxVolume = -80;

    public int currentParticipantTurn = 0;                  // how many scenearios did he play
    public int currentParticipantID = 1;
    public string currentParticipantGender  = "No Answer";
    #endregion
    #region status vars
    public ParticipantData ownParticipant;
    public GeneratedTasks generatedTasks;

    public int correctAnswers = 0;
    public int wrongAnswers = 0;
    public int skippedAnswers = 0;

    public int gameMode = 0;                                // 0:= Singleplayer; 1:= HalbCoop; 2:= Versus; 3:= Versus 2; 4:= Party; 5:= Collab
    public int nGameMode = -1;
    public int ownConnectionID = 0;

    public List<NetworkObject> playerList = new List<NetworkObject>();
    
    public bool preGameRunning = false;
    public bool gameRunning = false;
    
    public bool isConnected = false;
    public bool isHost = false;

    public bool startPressed = false;
    public bool gmClientReady = false;
    public bool everybodyReady = false;
    #endregion

    // Generate tasks
    public void GenerateTasks()
    {
        generatedTasks = new GeneratedTasks();
        for(int j = 0; j < turnsToPlay; j++)
        {
            int[][] tasks = new int[taskPerTurn][];
            int i;

            for (i = 0; i < taskPerTurn; i++)
            {
                tasks[i] = new int[3];
                tasks[i][0] = UnityEngine.Random.Range(intervalMin, intervalMax);    // numberA
                tasks[i][1] = UnityEngine.Random.Range(0, 3);                        // op
                tasks[i][2] = UnityEngine.Random.Range(intervalMin, intervalMax);    // numberB
            }
            generatedTasks.turns.Add(tasks);
        }
        if (Directory.Exists(pathToSaveLocation))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Create(pathToSaveLocation + "/generatedTasks.dat");

            GeneratedTasks data = generatedTasks;

            bF.Serialize(file, data);
            file.Close();
        }
    }
    public void LoadGeneratedTasks()
    {
        if (File.Exists(pathToSaveLocation + "/generatedTasks.dat"))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Open(pathToSaveLocation + "/generatedTasks.dat", FileMode.Open);
            GeneratedTasks data = (GeneratedTasks)bF.Deserialize(file);
            file.Close();

            generatedTasks = data;
        }
        else
        {
            GenerateTasks();
        }
    }

    public bool CheckParticipantID()
    {
        string pathFolder;
        if (supervisor)
        {
            pathFolder = pathToSaveLocation + /*"Session_"
                                + DateTime.Today.ToString("dd-M-yyyy/") + */"Supervisor_Participant" + currentParticipantID
                                /*+ participant.startTime.ToString("/hh-mmtt")*/;
        }
        else
        {
            pathFolder = pathToSaveLocation + /*"Session_"
                                + DateTime.Today.ToString("dd-M-yyyy/") + */"Participant" + currentParticipantID
                                /*+ participant.startTime.ToString("/hh-mmtt")*/;
        }

        return Directory.Exists(pathFolder);
    }
    public void CreateUserData(ParticipantData participant)
    {
        string pathFolder;
        if (supervisor) 
        {
            pathFolder = pathToSaveLocation + /*"Session_"
                                + DateTime.Today.ToString("dd-M-yyyy/") + */"Supervisor_Participant" + participant.identification
                                /*+ participant.startTime.ToString("/hh-mmtt")*/;
        }
        else
        {
            pathFolder = pathToSaveLocation + /*"Session_"
                                + DateTime.Today.ToString("dd-M-yyyy/") + */"Participant" + participant.identification
                                /*+ participant.startTime.ToString("/hh-mmtt")*/;
        }

        try
        {
            // Determine whether the directory exists.
            //int i = 0;
            //while (Directory.Exists(pathFolder))
            //{
            //    i++;
            //    if (supervisor)
            //    {
            //        pathFolder = pathToSaveLocation + /*"Session_"
            //                            + DateTime.Today.ToString("dd-M-yyyy/") + */"Supervisor_Participant" + participant.identification
            //                            /*+ participant.startTime.ToString("/hh-mmtt") */+ "(" + i + ")";
            //    }
            //    else
            //    {
            //        pathFolder = pathToSaveLocation + /*"Session_"
            //                            + DateTime.Today.ToString("dd-M-yyyy/") + */"Participant" + participant.identification
            //                            /*+ participant.startTime.ToString("/hh-mmtt") */+ "(" + i + ")";
            //    }               
            //}

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
        string path;

        if (supervisor)
        {
            path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + ".txt";
        }
        else
        {
            path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + ".txt";
        }
        int x = 0;
        while (File.Exists(path))
        {
            x++;
            if (supervisor)
            {
                path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + ".txt";
            }
            else
            {
                path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + ".txt";
            }
        }
        using (StreamWriter file = new StreamWriter(path, true))
        {
            file.WriteLine(DateTime.Today.ToString("D") + ", " + participant.startTime.ToString("h:mm:ss tt"));
            if (supervisor)
            {
                file.WriteLine("Supervisor_Participant " + participant.identification);
            }
            else
            {
                file.WriteLine("Participant " + participant.identification);
            }
            file.WriteLine("Gender:  " + participant.gender);
            file.WriteLine("" + currentParticipantTurn + ". Turn");
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
                    file.WriteLine("Versus 2");
                    break;
                case 4:
                    file.WriteLine("Party");
                    break;
                case 5:
                    file.WriteLine("Collab");
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
                file.WriteLine("Start at: " + task.startTime.ToString("N2"));                
                file.WriteLine("End at: " + task.endTime.ToString("N2"));
                file.WriteLine(task.task);
                file.WriteLine("time needed in seconds: " + task.timeNeeded.ToString("N2"));
                file.WriteLine("solved: " + task.solved);
                file.WriteLine("correct answer: " + task.correctAnswer);
                file.WriteLine("is the player behind: " + task.behind);
                file.WriteLine("Enemy score at this point: ");
                for(int j = 0; j < task.enemyIDs.Count; j++)
                {
                    file.WriteLine("    Player " + task.enemyIDs[j] + " : " + task.enemyScore[j]);
                }
                file.WriteLine("**********************************");
            }

        }

        if (supervisor)
        {
            path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + ".csv";
        }
        else
        {
            path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + ".csv";
        }
        x = 0;
        while (File.Exists(path))
        {
            x++;
            if (supervisor)
            {
                path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + ".csv";
            }
            else
            {
                path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + ".csv";
            }
        }
        using (StreamWriter fileCSV = new StreamWriter(path, true))
        {
            fileCSV.WriteLine("Task ID;Participant ID;Gender;Turn;Modus;TimestampStart;TimestampEnd;Duration;Behind;Point;Supervisor");
            string line = "";
            int i = 0;
            foreach (TaskData task in participant.tasks)
            {
                line += "" + (i++) + ";";
                line += participant.identification + ";";
                line += participant.gender + ";";
                line += currentParticipantTurn + ";";
                switch (participant.gameMode)
                {
                    case 0:
                        line += "Singleplayer;";
                        break;
                    case 1:
                        line += "Halb-Coop;";
                        break;
                    case 2:
                        line += "Versus;";
                        break;
                    case 3:
                        line += "Versus 2;";
                        break;
                    case 4:
                        line += "Party;";
                        break;
                    case 5:
                        line += "Collab;";
                        break;
                    default:
                        line += "mode error;";
                        break;
                }
                line += task.startTime.ToString("N2") + ";";
                line += task.endTime.ToString("N2") + ";";
                line += task.timeNeeded.ToString("N2") + ";";
                line += task.behind + ";";
                if (task.solved)
                {
                    if (task.correctAnswer)
                    {
                        line += "+1";
                    }
                    else
                    {
                        line += "-1";
                    }
                }
                else
                {
                    line += "0";
                }
                line += ";" + supervisor;
                fileCSV.WriteLine(line);
                line = "";
            }
        }

        // Creating a serialized file of particpant data
        BinaryFormatter bF = new BinaryFormatter();
        if (supervisor)
        {
            path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + "_NHR.dat";
        }
        else
        {
            path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + "_NHR.dat";
        }
        x = 0;
        while (File.Exists(path))
        {
            x++;
            if (supervisor)
            {
                path = pathFolder + "/Supervisor_Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + "_NHR.dat";
            }
            else
            {
                path = pathFolder + "/Participant" + participant.identification + "_Turn" + currentParticipantTurn + "(" + x + ")" + "_NHR.dat";
            }
        }
        FileStream fileB = File.Create(path);

        ParticipantData data = participant;

        bF.Serialize(fileB, data);
        fileB.Close();

    }

    // Game Settings
    public void SaveOptions()
    {
        if (Directory.Exists(pathToSaveLocation))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "options.dat");

            OptionsData data = new OptionsData(intervalMin, intervalMax, turnsToPlay, taskPerTurn, randomTasks, showScore, showTimer, timeLimit, pathToSaveLocation, musicVolume, sfxVolume);
        
            bF.Serialize(file, data);
            file.Close();
        }
    }
    public void LoadOptions()
    {
        if (File.Exists(Application.persistentDataPath + "options.dat"))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "options.dat", FileMode.Open);
            OptionsData data = (OptionsData)bF.Deserialize(file);
            file.Close();

            intervalMin = data.intervalMin;
            intervalMax = data.intervalMax;
            turnsToPlay = data.turnsToPlay;
            taskPerTurn = data.tasksPerTurn;
            randomTasks = data.randomTasks;
            showScore = data.showScore;
            showTimer = data.showTimer;
            timeLimit = data.timeLimit;
            pathToSaveLocation = data.pathToSaveLocation;
            musicVolume = data.musicVolume;
            sfxVolume = data.sfxVolume;
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
    public string gender;
    public int turn;
    public int connectionID;
    public int gameMode;
    public DateTime startTime;
    public float timePlayed;
    public int skippedTasks;
    public int correctAnswers;
    public int wrongAnswers;
    public List<TaskData> tasks;

    public ParticipantData(int gM, DateTime sT)
    {
        identification = GameManager.Instance.currentParticipantID;
        gender = GameManager.Instance.currentParticipantGender;
        turn = GameManager.Instance.currentParticipantTurn;
        connectionID = GameManager.Instance.ownConnectionID;
        gameMode = gM;
        startTime = sT;
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
    public float startTime;
    public float endTime;
    public float timeNeeded;
    public string task;
    public bool solved;
    public bool correctAnswer;
    public bool behind;
    public List<int> enemyIDs;
    public List<int> enemyScore;

    public TaskData(float sT, float eT, string ta, bool s, bool c, bool b, List<int> eID,List<int> eS)
    {
        startTime = sT;
        endTime = eT;
        timeNeeded = eT - sT;
        task = ta;
        solved = s;
        correctAnswer = c;
        behind = b;
        enemyIDs = eID;
        enemyScore = eS;
    }
}

[Serializable]
public class GeneratedTasks
{
    public List<int[][]> turns;

    public GeneratedTasks()
    {
        turns = new List<int[][]>();
    }
}

[Serializable]
public class OptionsData
{
    public int intervalMin, intervalMax;
    public int turnsToPlay;
    public int tasksPerTurn;
    public bool randomTasks;
    public bool showScore;
    public bool showTimer;
    public float timeLimit;
    public string pathToSaveLocation;
    public float musicVolume;
    public float sfxVolume;

    public OptionsData(int min, int max, int turns, int tPT, bool rT, bool shS, bool sT, float t, string p, float m, float sfx)
    {
        intervalMin = min;
        intervalMax = max;
        turnsToPlay = turns;
        tasksPerTurn = tPT;
        randomTasks = rT;
        showScore = shS;
        showTimer = sT;
        timeLimit = t;
        pathToSaveLocation = p;
        musicVolume = m;
        sfxVolume = sfx;
    }
}

