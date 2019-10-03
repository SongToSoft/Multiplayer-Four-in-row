using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum EState
{
    CHOSE_MODE,
    WAIT_PLAYERS,
    PLAYER_STEP,
    MOVE_CHIP,
    CHECK_WINNER,
    PLAYER_WIN,
    DRAW,
    WAIT_CHOSE
}

public enum EMode
{
    NON_SELECT,
    HOT_SEAT,
    ONLINE
}

public class FieldControl : MonoBehaviour
{
    [SerializeField]
    private GameObject choseModePanel,  newGamePanel;
    [SerializeField]
    private Text redScore, blueScore, redWin, blueWin;
    public static EChipColor startColorStep = EChipColor.RED;
    [SerializeField]
    private List<Column> columns;
    private Chip[,] chips;
    private EState state = EState.CHOSE_MODE;
    private int lastChipIdJ, lastChipIdI = 0;
    [SerializeField]
    private Player[] players;
    [SerializeField]
    private CustomNetworkManager customNetworkManager;
    public static int redScoreValue = 0, blueScoreValue = 0;

    public static EChipColor currentColorStep = EChipColor.RED;
    public static EMode mode = EMode.NON_SELECT;

    void Start()
    {
        chips = new Chip[6, 7];
        redScore.text = redScoreValue.ToString();
        blueScore.text = blueScoreValue.ToString();
        currentColorStep = startColorStep;
        ChangeStartColor();
        if (mode == EMode.NON_SELECT)
        {
            choseModePanel.SetActive(true);
            state = EState.CHOSE_MODE;
        }
        else
        {
            choseModePanel.SetActive(false);
            state = EState.PLAYER_STEP; ;
        }
    }

    void Update()
    {
        if (state != EState.CHOSE_MODE)
        {
            if (state == EState.WAIT_PLAYERS)
            {
                if (customNetworkManager.IsServer() && customNetworkManager.players.Count == 2)
                {
                    state = EState.PLAYER_STEP;
                    customNetworkManager.InstantiateCSC();
                    ClientServerConnector.Instanse.clientRequest = -1;
                    ClientServerConnector.Instanse.hostRequest = -1;
                    return;
                }
                if (!customNetworkManager.IsServer())
                {
                    players = Object.FindObjectsOfType<Player>();
                    if (players.Length == 2)
                    {
                        state = EState.PLAYER_STEP;
                        ClientServerConnector.Instanse.clientRequest = -1;
                        ClientServerConnector.Instanse.hostRequest = -1;
                    }
                    return;
                }
            }
            if (state == EState.PLAYER_STEP)
            {
                if (mode == EMode.HOT_SEAT)
                {
                    MouseControl();
                }
                else
                {
                    if (customNetworkManager.IsServer())
                    {
                        if (ClientServerConnector.Instanse.clientRequest != (-1))
                        {
                            Debug.Log("Server: Client Requst is: " + ClientServerConnector.Instanse.clientRequest);
                            AddChip(ClientServerConnector.Instanse.clientRequest);
                            ClientServerConnector.Instanse.clientRequest = -1;
                            customNetworkManager.GetLocalPlayer().SetActive(true);
                            customNetworkManager.GetDistancePlayer().SetActive(false);
                            state = EState.MOVE_CHIP;
                        }
                        else
                        {
                            if (customNetworkManager.GetLocalPlayer().GetActive())
                            {
                                MouseControl();
                            }
                        }
                    }
                    else
                    {
                        if (ClientServerConnector.Instanse.hostRequest != (-1))
                        {
                            Debug.Log("Client: Host Requst is: " + ClientServerConnector.Instanse.hostRequest);
                            AddChip(ClientServerConnector.Instanse.hostRequest);
                            GetLocalPlayer().CmdChangeHostRequest(-1);
                            GetLocalPlayer().SetActive(true);
                            GetDistancePlayer().SetActive(false);
                            state = EState.MOVE_CHIP;
                            //customNetworkManager.GetLocalPlayer().SetActive(!customNetworkManager.GetLocalPlayer().GetActive());
                            //customNetworkManager.GetDistancePlayer().SetActive(!customNetworkManager.GetDistancePlayer().GetActive());
                        }
                        else
                        {
                            if (GetLocalPlayer().GetActive())
                            {
                                MouseControl();
                            }
                        }
                    }
                }
            }
            if (state == EState.MOVE_CHIP)
            {
                if (chips[lastChipIdI, lastChipIdJ].IsMove())
                {
                    chips[lastChipIdI, lastChipIdJ].MoveToNewPosition();
                }
                else
                {
                    state = EState.CHECK_WINNER;
                }
                return;
            }
            if (state == EState.CHECK_WINNER)
            {
                for (int i = 0; i < 6; ++i)
                {
                    for (int j = 0; j < 7; ++j)
                    {
                        if (chips[i, j] != null)
                        {
                            EChipColor currentColor = chips[i, j].GetColor();
                            if (CheckHorizontal(currentColor, i, j) ||
                                CheckVertical(currentColor, i, j) ||
                                CheckRightDiagonal(currentColor, i, j) ||
                                CheckLeftDiagonal(currentColor, i, j))
                            {
                                state = EState.PLAYER_WIN;
                                return;
                            }
                            if (CheckFoolChips())
                            {
                                state = EState.DRAW;
                                return;
                            }
                        }
                    }
                }
                state = EState.PLAYER_STEP;
                ChangeCurrentColor();
                return;
            }
            if (state == EState.PLAYER_WIN)
            {
                if (currentColorStep == EChipColor.RED)
                {
                    ++redScoreValue;
                    if (mode == EMode.ONLINE)
                    {
                        if (customNetworkManager.IsServer())
                        {
                            customNetworkManager.GetLocalPlayer().IncreaseScore();
                        }
                        else
                        {
                            GetDistancePlayer().IncreaseScore();
                        }
                    }
                    redWin.text = "Winner";
                    blueWin.text = "Loser";
                }
                else
                {
                    ++blueScoreValue;
                    if (mode == EMode.ONLINE)
                    {
                        if (customNetworkManager.IsServer())
                        {
                            customNetworkManager.GetDistancePlayer().IncreaseScore();
                        }
                        else
                        {
                            GetLocalPlayer().IncreaseScore();
                        }
                    }
                    redWin.text = "Loser";
                    blueWin.text = "Winner";
                }
                newGamePanel.SetActive(true);
                state = EState.WAIT_CHOSE;
                return;
            }
            if (state == EState.DRAW)
            {
                redWin.text = "Draw";
                blueWin.text = "Draw";
                newGamePanel.SetActive(true);
                state = EState.WAIT_CHOSE;
                return;
            }
        }
    }

    private bool CheckFoolChips()
    {
        for (int i = 0; i < 6; ++i)
        {
            for (int j = 0; j < 7; ++j)
            {
                if (chips[i, j] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool CheckHorizontal(EChipColor currentColor, int i, int j)
    {
        int count = 1;
        for (int k = j + 1; k < 7; ++k)
        {
            if (chips[i, k] != null)
            {
                if (chips[i, k].GetColor() == currentColor)
                {
                    ++count;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        if (count >= 4)
        {
            for (int k = j; k < j + count; ++k)
            {
                chips[i, k].SetPointActive();
                if (mode == EMode.ONLINE)
                {
                    Debug.Log("RpcSetPointActive");
                    chips[i, k].RpcSetPointActive();
                }
            }
            return true;
        }
        return false;
    }

    private bool CheckVertical(EChipColor currentColor, int i, int j)
    {
        int count = 1;
        for (int k = i + 1; k < 6; ++k)
        {
            if (chips[k, j] != null)
            {
                if (chips[k, j].GetColor() == currentColor)
                {
                    ++count;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        if (count >= 4)
        {
            for (int k = i; k < i + count; ++k)
            {
                chips[k, j].SetPointActive();
            }
            return true;
        }
        return false;
    }

    private bool CheckRightDiagonal(EChipColor currentColor, int i, int j)
    {
        int count = 1;
        for (int k = 1; k < 7; ++k)
        {
            if ((i + k < 6) && (j + k < 7))
            {
                if (chips[i + k, j + k] != null)
                {
                    if (chips[i + k, j + k].GetColor() == currentColor)
                    {
                        ++count;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        if (count >= 4)
        {
            for (int k = 0; k < count; ++k)
            {
                chips[i + k, j + k].SetPointActive();
            }
            return true;
        }
        return false;
    }

    private bool CheckLeftDiagonal(EChipColor currentColor, int i, int j)
    {
        int count = 1;
        for (int k = 1; k < 7; ++k)
        {
            if ((i + k < 6) && (j - k >= 0))
            {
                //Debug.Log(i + " -- " + j);
                if (chips[i + k, j - k] != null)
                {
                    if (chips[i + k, j - k].GetColor() == currentColor)
                    {
                        //Debug.Log("Left Diagonal");
                        ++count;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        if (count >= 4)
        {
            // Debug.Log(i + "--" + j);
            for (int k = 0; k < count; ++k)
            {
                chips[i + k, j - k].SetPointActive();
            }
            return true;
        }
        return false;
    }

    private void MoveChips()
    {
        chips[lastChipIdI, lastChipIdJ].MoveToNewPosition();
    }

    private bool AddChip(int id)
    {
        int count = -1;
        for (int i = 0; i < 6; ++i)
        {
            if (chips[i, id] == null)
            {
                ++count;
            }
            else
            {
                break;
            }
        }
        if (count == -1)
        {
            return false;
        }
        chips[count, id] = Instantiate(GetCurrentChip(), columns[id].GetTopPosition(), GetCurrentChip().transform.rotation);
        for (int i = 0; i <= count; ++i)
        {
            chips[count, id].MoveDown();
        }
        lastChipIdI = count;
        lastChipIdJ = id;
        return true;
    }
    
    public void ChoseModeHotSeat()
    {
        state = EState.PLAYER_STEP;
        mode = EMode.HOT_SEAT;
        choseModePanel.SetActive(false);
    }

    public void ChoseModeOnline()
    {
        state = EState.WAIT_PLAYERS;
        mode = EMode.ONLINE;
        choseModePanel.SetActive(false);
        customNetworkManager.gameObject.SetActive(true);
    }

    public void NewGame()
    {
        if (mode == EMode.HOT_SEAT)
        {
            SceneManager.LoadScene("MainGame");
        }
        else
        {
            newGamePanel.SetActive(false);
            currentColorStep = startColorStep;
            ChangeStartColor();
            if (customNetworkManager.IsServer())
            {
                redScore.text = customNetworkManager.players[0].GetScore().ToString();
                blueScore.text = customNetworkManager.players[1].GetScore().ToString();
                customNetworkManager.GetLocalPlayer().CmdChangeClientRequest(-1);
                customNetworkManager.GetLocalPlayer().CmdChangeHostRequest(-1);
                if (currentColorStep == EChipColor.RED)
                {
                    customNetworkManager.GetLocalPlayer().SetActive(true);
                    customNetworkManager.GetDistancePlayer().SetActive(false);
                }
                else
                {
                    customNetworkManager.GetLocalPlayer().SetActive(false);
                    customNetworkManager.GetDistancePlayer().SetActive(true);
                }
            }
            else
            {
                redScore.text = players[0].GetScore().ToString();
                blueScore.text = players[1].GetScore().ToString();
                GetLocalPlayer().CmdChangeClientRequest(-1);
                GetLocalPlayer().CmdChangeHostRequest(-1);
                if (currentColorStep == EChipColor.RED)
                {
                    GetDistancePlayer().SetActive(true);
                    GetLocalPlayer().SetActive(false);
                }
                else
                {
                    GetDistancePlayer().SetActive(false);
                    GetLocalPlayer().SetActive(true);
                }
            }
            redWin.text = "";
            blueWin.text = "";
            DelChips();
            state = EState.PLAYER_STEP;
        }
    }

    public void ChangeStartColor()
    {
        startColorStep = (startColorStep == EChipColor.RED) ? EChipColor.BLUE : EChipColor.RED;
    }

    public void ChangeCurrentColor()
    {
        currentColorStep = (currentColorStep == EChipColor.RED) ? EChipColor.BLUE : EChipColor.RED;
    }

    public Chip GetCurrentChip()
    {
        return ((currentColorStep == EChipColor.RED) ? StaticContent.redChip : StaticContent.blueChip);
    }

    public Player GetLocalPlayer()
    {
        if (players.Length == 2)
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
        if (players.Length == 2)
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

    public void DelChips()
    {
        for (int i = 0; i < 6; ++i)
        {
            for (int j = 0; j < 7; ++j)
            {
                if (chips[i, j] != null)
                {
                    Destroy(chips[i, j].gameObject);
                }
            }
        }
    }

    public void MouseControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null)
            {
                Column column = hit.transform.GetComponent<Column>();
                if (AddChip(column.GetId()))
                {
                    if (mode == EMode.ONLINE)
                    {
                        if (customNetworkManager.IsServer())
                        {
                            customNetworkManager.GetLocalPlayer().CmdChangeHostRequest(column.GetId());
                            customNetworkManager.GetLocalPlayer().SetActive(false);
                            customNetworkManager.GetDistancePlayer().SetActive(true);
                        }
                        else
                        {
                            GetLocalPlayer().CmdChangeClientRequest(column.GetId());
                            GetLocalPlayer().SetActive(false);
                            GetDistancePlayer().SetActive(true);
                        }
                    }                   
                    state = EState.MOVE_CHIP;
                }
            }
        }
        return;
    }
}
