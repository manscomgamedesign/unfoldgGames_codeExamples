using UnityEngine;

public class MercenaryPlacementIdentity : MonoBehaviour
{
    public float x;
    public float y;
    public float z;

    public void SetLocation3D(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public void SetLocation2D(float x, float z)
    {
        this.x = x;
        this.z = z;
    }

    public Vector3 GetLocation()
    {
        return new Vector3(x, 0, z);
    }
}
