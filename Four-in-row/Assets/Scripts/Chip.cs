using UnityEngine;
using UnityEngine.Networking;

public enum EChipColor
{
    RED,
    BLUE
}

public class Chip : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    private GameObject point;
    [SerializeField]
    private EChipColor color;
    private Transform chipTransform;
    private Vector3 newPosition;

    public EChipColor GetColor()
    {
        return color;
    }

    protected void Awake()
    {
        chipTransform = GetComponent<Transform>();
        newPosition = chipTransform.position;
    }

    public void MoveDown()
    {
        newPosition.y = newPosition.y - 3.2f;
    }

    public void MoveToNewPosition()
    {
        if (chipTransform.position != newPosition)
        {
            chipTransform.position = Vector3.MoveTowards(chipTransform.position, newPosition, StaticContent.chipSpeed * Time.deltaTime);
        }
    }

    public bool IsMove()
    {
        return (chipTransform.position != newPosition);
    }

    public void SetPointActive()
    {
        point.SetActive(true);
    }

    [ClientRpc]
    public void RpcSetPointActive()
    {
        point.SetActive(true);
    }
}
