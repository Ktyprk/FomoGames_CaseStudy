using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public int ColorId { get; private set; }
    private int row;
    private int col;
    private int direction;
    private Color exitColor;

    [SerializeField] private ParticleSystem particleSystem;
    private ParticleSystemRenderer particleSystemRenderer;

    [Header("Animator")]
    [SerializeField] private Animator exitAnimator;  
    private int animHide = Animator.StringToHash("Hide");

    public void Initialize(ExitInfo info, Color visualColor, Material exitMaterial)
    {
        ColorId = info.Colors;
        row = info.Row;
        col = info.Col;
        direction = info.Direction;
        exitColor = visualColor;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material mat = new Material(exitMaterial);
            mat.color = visualColor;
            renderer.material = mat;
        }

        SetupExitVisual();
        SetupParticleSystem();
    }

    void SetupExitVisual()
    {
        float rotation = 0;

        switch (direction)
        {
            case 0: rotation = 0; break;     // Up
            case 1: rotation = 90; break;    // Right
            case 2: rotation = 180; break;   // Down
            case 3: rotation = 270; break;   // Left
        }

        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    void SetupParticleSystem()
    {
        if (particleSystem != null)
        {
            particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleSystemRenderer != null)
            {
                Material particleMat = new Material(particleSystemRenderer.material);
                particleMat.color = exitColor;
                particleSystemRenderer.material = particleMat;
            }

            var main = particleSystem.main;
            main.startColor = exitColor;
            particleSystem.Stop();
        }
    }

    public void PlayExitParticle()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
            Debug.Log($"Exit particle oynatılıyor - Renk ID: {ColorId}");
        }

        if (exitAnimator != null)
        {
            exitAnimator.SetTrigger(animHide);
            Debug.Log("Exit Animator -> Hide tetiklendi");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Block block = other.GetComponent<Block>();
        if (block != null && block.ColorId == ColorId)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBlockReachedExit(block, this);
            }
        }
    }
}
