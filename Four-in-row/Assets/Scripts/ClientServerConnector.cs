using UnityEngine;
using UnityEngine.Networking;

public class ClientServerConnector : NetworkBehaviour
{
    #region SingleTon
    public static ClientServerConnector Instanse { get; private set; }
    #endregion

    [SyncVar]
    [SerializeField]
    public int clientRequest;

    [SyncVar]
    [SerializeField]
    public int hostRequest;

    private void Awake()
    {
        Instanse = this;
    }

    [Command]
    public void CmdSetClientRequest(int _clientRequest)
    {
        clientRequest = _clientRequest;
        RpcSetClientRequest(_clientRequest);
    }

    [ClientRpc]
    public void RpcSetClientRequest(int _clientRequest)
    {
        clientRequest = _clientRequest;
    }
}
