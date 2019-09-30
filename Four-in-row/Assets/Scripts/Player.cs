using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar]
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
    }
}
