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

    public void Initialize(ExitInfo info, Color visualColor, Material exitMaterial)
    {
        ColorId = info.Colors;
        row = info.Row;
        col = info.Col;
        direction = info.Direction;
        exitColor = visualColor;
        
        // Exit materyalini ayarla
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

    void SetupParticleSystem()
    {
        if (particleSystem != null)
        {
            // Particle System Renderer'ı al
            particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleSystemRenderer != null)
            {
                // Material rengini exit rengiyle eşleştir
                Material particleMat = new Material(particleSystemRenderer.material);
                particleMat.color = exitColor;
                particleSystemRenderer.material = particleMat;
            }

            // Particle system main modülünü ayarla
            var main = particleSystem.main;
            main.startColor = exitColor;
            
            // Başlangıçta kapalı
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
    }

    private void OnTriggerEnter(Collider other)
    {
        Block block = other.GetComponent<Block>();
        if (block != null && block.ColorId == ColorId)
        {
            // GameManager'a block ve exit'i bildir
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBlockReachedExit(block, this);
            }
        }
    }
}