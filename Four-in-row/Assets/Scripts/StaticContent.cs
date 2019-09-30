using UnityEngine;

public static class StaticContent
{
    static public float chipSpeed = 30f;
    public static Chip redChip = Resources.Load<Chip>("Red Сhip");
    public static Chip blueChip = Resources.Load<Chip>("Blue Сhip");
    public static ClientServerConnector clientServerConnector = Resources.Load<ClientServerConnector>("ClienServerConnector");
}
