using UnityEngine;

public class Column : MonoBehaviour
{
    [SerializeField]
    private int id;
    private BoxCollider2D columnCollider;
    private Transform columnTransofrm;

    private void Awake()
    {
        columnCollider = GetComponent<BoxCollider2D>();
        columnTransofrm = GetComponent<Transform>();
    }

    public int GetId()
    {
        return id;
    }

    public BoxCollider2D GetCollider()
    {
        return columnCollider;
    }

    public Vector3 GetTopPosition()
    {
        return new Vector3(columnTransofrm.position.x, 11.2f, 95);
    }
}
