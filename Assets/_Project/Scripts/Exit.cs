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
        
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
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
        Vector3 scale = Vector3.one;

        switch (direction)
        {
            case 0: // Up
                rotation = 0;
                scale = new Vector3(0.3f, 0.2f, 0.15f);
                break;
            case 1: // Right
                rotation = 90;
                scale = new Vector3(0.3f, 0.2f, 0.15f);
                break;
            case 2: // Down
                rotation = 180;
                scale = new Vector3(0.3f, 0.2f, 0.15f);
                break;
            case 3: // Left
                rotation = 270;
                scale = new Vector3(0.3f, 0.2f, 0.15f);
                break;
        }

        transform.rotation = Quaternion.Euler(0, rotation, 0);
        transform.localScale = scale;
    }
}