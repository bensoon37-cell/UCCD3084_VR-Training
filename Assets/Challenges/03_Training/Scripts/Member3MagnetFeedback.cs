using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
public sealed class Member3MagnetFeedback : MonoBehaviour
{
    [SerializeField] private Color inactiveColor = new Color(0.8f, 0.08f, 0.05f, 1f);
    [SerializeField] private Color activeColor = new Color(0.05f, 0.85f, 0.18f, 1f);
    [SerializeField] private Renderer stateRenderer;
    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private ParticleSystem liftParticles;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    private bool lastActive;
    private bool lastLifting;

    private void Awake()
    {
        if (socket == null) socket = GetComponentInChildren<XRSocketInteractor>(true);
        if (stateRenderer == null)
        {
            Transform visual = transform.Find("Magnet");
            stateRenderer = visual != null ? visual.GetComponent<Renderer>() : GetComponentInChildren<Renderer>(true);
        }
        if (liftParticles == null) liftParticles = CreateLiftParticles();
        ConfigureSpatialAudio();
        Refresh(true);
    }

    private void Update() => Refresh(false);

    private void Refresh(bool force)
    {
        bool isActive = socket != null && socket.socketActive && socket.interactionLayers.value != 0;
        bool isLifting = isActive && socket.hasSelection;
        if (force || isActive != lastActive)
        {
            ApplyColor(isActive ? activeColor : inactiveColor);
            lastActive = isActive;
        }
        if (force || isLifting != lastLifting)
        {
            SetParticles(isLifting);
            lastLifting = isLifting;
        }
    }

    private void ApplyColor(Color color)
    {
        if (stateRenderer == null) return;
        stateRenderer.GetPropertyBlock(propertyBlock);
        Material material = stateRenderer.sharedMaterial;
        if (material != null && material.HasProperty(BaseColorId)) propertyBlock.SetColor(BaseColorId, color);
        if (material != null && material.HasProperty(ColorId)) propertyBlock.SetColor(ColorId, color);
        stateRenderer.SetPropertyBlock(propertyBlock);
    }

    private void SetParticles(bool shouldPlay)
    {
        if (liftParticles == null) return;
        if (shouldPlay && !liftParticles.isPlaying) liftParticles.Play();
        else if (!shouldPlay) liftParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private ParticleSystem CreateLiftParticles()
    {
        GameObject particlesObject = new GameObject("Lift Particles (Member 3)");
        particlesObject.transform.SetParent(socket != null ? socket.transform : transform, false);
        particlesObject.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        ParticleSystem particles = particlesObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 0.45f;
        main.startSpeed = 0.35f;
        main.startSize = 0.045f;
        main.startColor = new Color(0.15f, 0.85f, 1f, 0.9f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 28f;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f;
        return particles;
    }

    private void ConfigureSpatialAudio()
    {
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>(true))
        {
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 18f;
            source.dopplerLevel = 0f;
        }
    }
}
