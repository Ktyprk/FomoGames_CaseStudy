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
            
            if (Length == 1)
                transform.localScale = new Vector3(blockWidth, blockHeight, blockWidth);
            else
                transform.localScale = new Vector3(blockWidth, blockHeight, blockWidth * 2);
        }
        else if (isHorizontal && !isVertical)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            
            if (Length == 1)
                transform.localScale = new Vector3(blockWidth, blockHeight, blockWidth);
            else
                transform.localScale = new Vector3(blockWidth * 2, blockHeight, blockWidth);
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
        float enter;
        
        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 delta = hitPoint - dragStartPos;

            Vector3 constrainedDelta = ConstrainMovement(delta);
            Vector3 targetPos = dragStartPos + constrainedDelta;
            
            Vector2Int targetCell = WorldToGrid(targetPos);
            
            if (CanMoveTo(targetCell))
            {
                transform.position = gameManager.GetWorldPosition(targetCell.x, targetCell.y);
                Row = targetCell.x;
                Col = targetCell.y;
            }
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;

        if (Row != dragStartCell.x || Col != dragStartCell.y)
        {
            gameManager.IncrementMoveCount();
            CheckExitReached();
        }
        else
        {
            transform.position = dragStartPos;
        }
    }

    Vector3 ConstrainMovement(Vector3 delta)
    {
        if (isVertical && !isHorizontal)
        {
            return new Vector3(0, 0, delta.z);
        }
        else if (isHorizontal && !isVertical)
        {
            return new Vector3(delta.x, 0, 0);
        }

        return delta;
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt(worldPos.x / gameManager.cellSize);
        int row = Mathf.RoundToInt(-worldPos.z / gameManager.cellSize);
        return new Vector2Int(row, col);
    }

    bool CanMoveTo(Vector2Int targetCell)
    {
        if (targetCell.x == Row && targetCell.y == Col)
            return true;

        if (!gameManager.IsValidPosition(targetCell.x, targetCell.y))
            return false;

        if (gameManager.IsBlockAt(targetCell, this))
            return false;

        int direction = GetDirection(new Vector2Int(Row, Col), targetCell);
        if (direction == -1 || !allowedDirections.Contains(direction))
            return false;

        return true;
    }

    int GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        
        if (delta.x < 0 && delta.y == 0) return 0; // Up
        if (delta.y > 0 && delta.x == 0) return 1; // Right
        if (delta.x > 0 && delta.y == 0) return 2; // Down
        if (delta.y < 0 && delta.x == 0) return 3; // Left
        
        return -1;
    }

    void CheckExitReached()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.3f);
        foreach (var col in colliders)
        {
            ExitTrigger exit = col.GetComponent<ExitTrigger>();
            if (exit != null && exit.ColorId == ColorId)
            {
                StartCoroutine(ExitAnimation(exit.transform.position));
                return;
            }
        }
    }

    IEnumerator ExitAnimation(Vector3 exitPos)
    {
        IsMoving = true;
        float duration = 0.3f;
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            transform.position = Vector3.Lerp(startPos, exitPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }

        gameManager.OnBlockReachedExit(this);
    }
}