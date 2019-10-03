using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    private bool active;

    [SyncVar]
    private int score;

    public bool GetActive()
    {
        return active;
    }

    public void SetActive(bool _active)
    {
        active = _active;
    }

    public void IncreaseScore()
    {
        ++score;
    }

    public int GetScore()
    {
        return score;
    }

    public bool IsLocal()
    {
        return isLocalPlayer;
    }

    [Command]
    public void CmdChangeClientRequest(int value)
    {
        ClientServerConnector.Instanse.clientRequest = value;
        RpcChangeClientRequest(value);
    }

    [ClientRpc]
    public void RpcChangeClientRequest(int value)
    {
        ClientServerConnector.Instanse.clientRequest = value;
    }

    [Command]
    public void CmdChangeHostRequest(int value)
    {
        ClientServerConnector.Instanse.hostRequest = value;
        RpcChangeHostRequest(value);
    }

    [ClientRpc]
    public void RpcChangeHostRequest(int value)
    {
        ClientServerConnector.Instanse.hostRequest = value;
    }
}
