using UnityEngine;
using UnityEngine.Networking;

public enum EChipColor
{
    RED,
    BLUE
}

public class Chip : NetworkBehaviour
{
    [SerializeField]
    private EChipColor color;
    private Transform chipTransform;
    private Vector3 newPosition;
    private SpriteRenderer spriteRenderer;

    protected void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        chipTransform = GetComponent<Transform>();
        newPosition = chipTransform.position;
    }

    public EChipColor GetColor()
    {
        return color;
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

    public void SetColorActive()
    {
        spriteRenderer.color = new Color(255, 255, 0);
    }

    [ClientRpc]
    public void RpcSetColorActive()
    {
        spriteRenderer.color = new Color(255, 255, 0);
    }
}
