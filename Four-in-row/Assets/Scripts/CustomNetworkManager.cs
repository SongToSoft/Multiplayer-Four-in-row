using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public List<Player> players = new List<Player>();
    //public List<Chip> chips = new List<Chip>();
    #region SingleTon
    public static CustomNetworkManager Instanse { get; private set; }
    #endregion

    private void Awake()
    {
        Instanse = this;    
    }

    public Chip InstantiateChip(Chip currentChip, Vector3 position)
    {
        Chip chip = Instantiate(currentChip, position, currentChip.transform.rotation);
        NetworkServer.Spawn(chip.gameObject);
        //NetworkServer.Destroy(
        //chips.Add(chip);
        return chip;
    }

    public void InstantiateCSC()
    {
        GameObject clientServerConnector = Instantiate(spawnPrefabs[2]);
        NetworkServer.Spawn(clientServerConnector);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        players.Add(player.GetComponent<Player>());
        if (players.Count == 1)
        {
            players[0].SetActive(true);
        }
        if (players.Count == 2)
        {
            players[1].SetActive(false);
        }
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public Player GetLocalPlayer()
    {
        if (players.Count == 2 )
        {
            if (players[0].IsLocal())
            {
                return players[0];
            }
            else
            {
                return players[1];
            }
        }
        return null;
    }

    public Player GetDistancePlayer()
    {
        if (players.Count == 2)
        {
            if (!players[0].IsLocal())
            {
                return players[0];
            }
            else
            {
                return players[1];
            }
        }
        return null;
    }

    public bool IsServer()
    {
        if (GetLocalPlayer() != null)
        {
            if (GetLocalPlayer().isServer)
            {
                return true;
            }
        }
        return false;
    }
}
