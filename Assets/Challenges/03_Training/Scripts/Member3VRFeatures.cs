using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
public sealed class Member3VRFeatures : MonoBehaviour
{
    private const string TrainingScene = "Training_Prototype_Broken";
    private const string VignettePreference = "Member3.VignetteEnabled";
    private readonly Dictionary<Rigidbody, Pose> initialPoses = new Dictionary<Rigidbody, Pose>();
    private readonly List<UnityEngine.XR.InputDevice> controllers = new List<UnityEngine.XR.InputDevice>();
    private TunnelingVignetteController vignette;
    private bool primaryButtonWasPressed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != TrainingScene || FindObjectOfType<Member3VRFeatures>() != null) return;
        new GameObject("Member 3 VR Features").AddComponent<Member3VRFeatures>();
    }

    private void Awake()
    {
        foreach (TeleportationProvider provider in FindObjectsOfType<TeleportationProvider>(true)) provider.enabled = true;
        foreach (SnapTurnProviderBase provider in FindObjectsOfType<SnapTurnProviderBase>(true)) provider.enabled = true;

        vignette = FindObjectOfType<TunnelingVignetteController>(true);
        SetVignette(PlayerPrefs.GetInt(VignettePreference, 1) != 0, false);
        CaptureResetState();
        WireResetButton();
        ConfigureSpatialAudio();
    }

    private void Update()
    {
        bool pressed = Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;
        controllers.Clear();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllers);
        bool primaryPressed = false;
        foreach (UnityEngine.XR.InputDevice controller in controllers)
        {
            if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value)
                primaryPressed = true;
        }
        if (pressed || (primaryPressed && !primaryButtonWasPressed)) ToggleVignette();
        primaryButtonWasPressed = primaryPressed;
    }

    public void ToggleVignette() => SetVignette(vignette == null || !vignette.enabled, true);

    public void ResetTraining()
    {
        foreach (KeyValuePair<Rigidbody, Pose> entry in initialPoses)
        {
            if (entry.Key == null) continue;
            entry.Key.velocity = Vector3.zero;
            entry.Key.angularVelocity = Vector3.zero;
            entry.Key.transform.SetPositionAndRotation(entry.Value.position, entry.Value.rotation);
            entry.Key.Sleep();
        }
    }

    private void SetVignette(bool enabled, bool save)
    {
        if (vignette != null) vignette.enabled = enabled;
        if (!save) return;
        PlayerPrefs.SetInt(VignettePreference, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CaptureResetState()
    {
        foreach (Rigidbody body in FindObjectsOfType<Rigidbody>(true))
            initialPoses[body] = new Pose(body.position, body.rotation);
    }

    private void WireResetButton()
    {
        foreach (XRButton button in FindObjectsOfType<XRButton>(true))
            if (button.name == "Reset Button") button.OnPress.AddListener(ResetTraining);
    }

    private static void ConfigureSpatialAudio()
    {
        foreach (AudioSource source in FindObjectsOfType<AudioSource>(true))
        {
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 18f;
            source.dopplerLevel = 0f;
        }
    }
}
