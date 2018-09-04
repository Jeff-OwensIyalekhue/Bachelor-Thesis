﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NetworkMenu : MonoBehaviour {

    NetworkManager networkManager;

    public TMP_Text networkInfo;
    public GameObject inputPanel;
    public Canvas canvas;


    // Use this for initialization
    void Start () {
        if (GameManager.Instance.isConnected)
            inputPanel.SetActive(false);

        networkManager = FindObjectOfType<NetworkManager>();
        if(networkManager.networkAddress != "localhost")
            networkInfo.text = networkManager.networkAddress + "\n" + networkManager.networkPort;
        else
            networkInfo.text = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[1] + "\n" + networkManager.networkPort;

        if(GameManager.Instance.isHost)
            networkInfo.text += " (H)";
    }

    // Update is called once per frame
    void Update () {
        //if (GameManager.Instance.nGameMode == 0 && GameManager.Instance.isConnected)
        //{
        //    if (!GameManager.Instance.isHost)
        //    {
        //        Debug.Log("Session is on Singleplayer");
        //        Disconnect();
        //    }
        //}
    }

    public void PlayAsHost()
    {
        networkManager.StartHost();
        networkInfo.text += " (H)";
        GameManager.Instance.isHost = true;
        GameManager.Instance.isConnected = true;
        inputPanel.SetActive(false);
        //canvas.sortingOrder = -2;
    }

    public void PlayAsClient()
    {
        networkManager.StartClient();
        GameManager.Instance.isConnected = true;
        inputPanel.SetActive(false);
        //canvas.sortingOrder = -2;
    }

    public void SetPort(int port)
    {
        networkManager.networkPort = port;
    }

    public void SetNetworkAddress(string ip)
    {
        networkManager.networkAddress = ip;

        if (networkManager.networkAddress != "localhost")
            networkInfo.text = networkManager.networkAddress + "\n" + networkManager.networkPort;
        else
            networkInfo.text = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[1] + "\n" + networkManager.networkPort;
    }

    public void Disconnect()
    {
        if (!GameManager.Instance.isConnected)
            return;
        GameManager.Instance.isConnected = false;
        GameManager.Instance.nGameMode = -1;
        if (GameManager.Instance.isHost)
        {
            GameManager.Instance.isHost = false;
            networkManager.StopHost();
        }
        else
        {
            networkManager.StopClient();
        }
        inputPanel.SetActive(true);
    }
}
