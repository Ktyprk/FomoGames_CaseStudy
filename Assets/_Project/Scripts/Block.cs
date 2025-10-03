using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int ColorId { get; private set; }
    public int Length { get; private set; }
    public bool IsMoving { get; private set; }

    private List<int> allowedDirections;
    private GameManager gameManager;
    private MeshRenderer meshRenderer;
    private Material blockMaterial;

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private Vector2Int dragStartCell;
    private Plane dragPlane;

    private bool isVertical;
    private bool isHorizontal;

    [SerializeField] private float dragThreshold = 0.3f;
    [SerializeField] private float moveSpeed = 8f;

    void Awake()
    {
        mainCamera = Camera.main;
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void Initialize(MovableInfo info, GameManager manager)
    {
        Row = info.Row;
        Col = info.Col;
        ColorId = info.Colors;
        Length = info.Length;
        allowedDirections = new List<int>(info.Direction);
        gameManager = manager;
        
        isVertical = allowedDirections.Contains(0) || allowedDirections.Contains(2);
        isHorizontal = allowedDirections.Contains(1) || allowedDirections.Contains(3);

        SetupBlockVisual();
        ApplyTexture();
    }

    void SetupBlockVisual()
    {
        float blockHeight = 0.5f;
        float blockWidth = 0.9f * gameManager.cellSize;
        
        if (isVertical && !isHorizontal)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
            transform.localScale = new Vector3(blockWidth, blockHeight, blockWidth);
        }
        else if (isHorizontal && !isVertical)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(blockWidth, blockHeight, blockWidth);
        }
    }

    void ApplyTexture()
    {
        if (gameManager.textureManager == null || meshRenderer == null)
            return;

        bool isParallel = isHorizontal; 
        Texture texture = gameManager.textureManager.GetTexture(ColorId, Length, isParallel);
        
        if (texture != null)
        {
            blockMaterial = new Material(gameManager.blockMaterial);
            blockMaterial.mainTexture = texture;
            meshRenderer.material = blockMaterial;
        }
    }

    void OnMouseDown()
    {
        if (IsMoving) return;

        isDragging = true;
        dragStartPos = transform.position;
        dragStartCell = new Vector2Int(Row, Col);
        dragPlane = new Plane(Vector3.up, transform.position);
    }

    void OnMouseDrag()
    {
        if (!isDragging || IsMoving) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 delta = hitPoint - dragStartPos;
        Vector3 constrained = ConstrainMovement(delta);

        int? dir = GetDragDirection(constrained);
        if (!dir.HasValue) return;

        int steps = ComputeMaxSteps(dir.Value);
        if (steps <= 0) return;

        StartCoroutine(SmoothMove(dir.Value, steps));
        isDragging = false;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    IEnumerator SmoothMove(int dir, int steps)
    {
        IsMoving = true;

        Vector2Int step = StepVec(dir);
        Vector2Int targetCell = new Vector2Int(Row, Col) + step * steps;
        Vector3 targetPos = gameManager.GetWorldPosition(targetCell.x, targetCell.y);

        float distance = Vector3.Distance(transform.position, targetPos);
        float duration = distance / moveSpeed;
        float elapsed = 0;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        Row = targetCell.x;
        Col = targetCell.y;

        IsMoving = false;

        gameManager.IncrementMoveCount();
        CheckExitReached();
    }

    int? GetDragDirection(Vector3 constrainedDelta)
    {
        bool vertical = allowedDirections.Contains(0) || allowedDirections.Contains(2);
        bool horizontal = allowedDirections.Contains(1) || allowedDirections.Contains(3);

        if (vertical && Mathf.Abs(constrainedDelta.z) > dragThreshold)
            return (constrainedDelta.z > 0f) ? 0 : 2;

        if (horizontal && Mathf.Abs(constrainedDelta.x) > dragThreshold)
            return (constrainedDelta.x > 0f) ? 1 : 3;

        return null;
    }

    Vector2Int StepVec(int dir)
    {
        switch (dir)
        {
            case 0: return new Vector2Int(-1, 0);  // Up
            case 1: return new Vector2Int(0, 1);   // Right
            case 2: return new Vector2Int(1, 0);   // Down
            case 3: return new Vector2Int(0, -1);  // Left
            default: return Vector2Int.zero;
        }
    }

    int ComputeMaxSteps(int dir)
    {
        Vector2Int step = StepVec(dir);
        Vector2Int curBase = new Vector2Int(Row, Col);
        int canMove = 0;

        while (true)
        {
            Vector2Int nextBase = curBase + step;
            List<Vector2Int> footprint = GetOccupiedCells(nextBase);

            if (!IsFootprintValid(footprint))
                break;

            canMove++;
            curBase = nextBase;
        }

        return canMove;
    }

    bool IsFootprintValid(List<Vector2Int> footprint)
    {
        foreach (var cell in footprint)
        {
            if (!gameManager.IsValidPosition(cell.x, cell.y))
                return false;

            if (gameManager.IsBlockAt(cell, this))
                return false;
        }
        return true;
    }

    Vector3 ConstrainMovement(Vector3 delta)
    {
        if (isVertical && !isHorizontal)
            return new Vector3(0, 0, delta.z);
        else if (isHorizontal && !isVertical)
            return new Vector3(delta.x, 0, 0);

        return delta;
    }

    List<Vector2Int> GetOccupiedCells(Vector2Int baseCell)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        cells.Add(baseCell);

        if (Length == 2)
        {
            if (isVertical && !isHorizontal)
                cells.Add(new Vector2Int(baseCell.x + 1, baseCell.y));
            else if (isHorizontal && !isVertical)
                cells.Add(new Vector2Int(baseCell.x, baseCell.y + 1));
        }

        return cells;
    }

    public List<Vector2Int> GetOccupiedCells()
    {
        return GetOccupiedCells(new Vector2Int(Row, Col));
    }

    void CheckExitReached()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (var col in colliders)
        {
            ExitTrigger exit = col.GetComponent<ExitTrigger>();
            if (exit != null && exit.ColorId == ColorId)
            {
                StartCoroutine(ExitAnimation(exit));
                return;
            }
        }
    }

    IEnumerator ExitAnimation(ExitTrigger exit)
    {
        IsMoving = true;
        float duration = 0.3f;
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 exitPos = exit.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            transform.position = Vector3.Lerp(startPos, exitPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }

        gameManager.OnBlockReachedExit(this, exit);
    }
}