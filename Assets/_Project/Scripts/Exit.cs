using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public int ColorId { get; private set; }
    private int row;
    private int col;
    private int direction;

    public void Initialize(ExitInfo info, Color visualColor, Material exitMaterial)
    {
        ColorId = info.Colors;
        row = info.Row;
        col = info.Col;
        direction = info.Direction;
        
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material mat = new Material(exitMaterial);
            mat.color = visualColor;
            renderer.material = mat;
        }

        SetupExitVisual();
    }

    void SetupExitVisual()
    {
        float rotation = 0;

        switch (direction)
        {
            case 0: // Up
                rotation = 0;
                break;
            case 1: // Right
                rotation = 90;
                break;
            case 2: // Down
                rotation = 180;
                break;
            case 3: // Left
                rotation = 270;
                break;
        }

        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
}