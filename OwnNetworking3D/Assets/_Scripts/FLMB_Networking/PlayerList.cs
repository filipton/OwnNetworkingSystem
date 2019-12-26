using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class PlayerList : MonoBehaviour
{
    public List<Player> Players = new List<Player>();
    NetworkManager _NetworkManager;

    private void Awake()
    {
        _NetworkManager = GetComponent<NetworkManager>();
    }

    void PrintAllPlayers()
    {
        string toPrint = "";
        for(int i = 0; i < Players.Count; i++)
        {
            toPrint += Players[i].NickName + " : ";
        }
        print($"Players Connected: {toPrint}");
    }

    [Rpc]
    public void AddClient(string Nick)
    {
        Players.Add(new Player { NickName = Nick });
        PrintAllPlayers();
    }

    [Rpc]
    public void RemoveClient(string Nick)
    {
        Players.Remove(new Player { NickName = Nick });
        PrintAllPlayers();
    }
}

[Serializable]
public class Player
{
    public string NickName;
    //Something else
}