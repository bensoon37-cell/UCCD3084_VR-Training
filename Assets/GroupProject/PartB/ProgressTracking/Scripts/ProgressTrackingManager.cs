using System.Collections;
using TMPro;
using UnityEngine;

public sealed class ProgressTrackingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text statusText;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip duplicateClip;
    [SerializeField] private AudioClip inactiveClip;
    [SerializeField] private ParticleSystem successParticles;

    [Header("Reset")]
    [SerializeField] private TrackableBox[] resettableBoxes;

    private readonly ProgressTrackingState state =
        new ProgressTrackingState();

    private float elapsedSeconds;
    private Coroutine statusCoroutine;

    private void Start()
    {
        RefreshUI("Press Start Session");
    }

    private void Update()
    {
        if (!state.IsRunning)
            return;

        elapsedSeconds += Time.deltaTime;
        UpdateTimeText();
    }

    public void StartSession()
    {
        StopStatusCoroutine();

        state.StartSession();
        elapsedSeconds = 0f;

        StopParticles();
        RefreshUI("Session running");
    }

    public void HandleCheckpointEntry(TrackableBox box)
    {
        if (box == null)
            return;

        if (!state.IsRunning)
        {
            PlayClip(inactiveClip);
            ShowTemporaryStatus("Press Start Session first");
            return;
        }

        int boxId = box.GetInstanceID();

        if (state.TryCountBox(boxId))
        {
            PlayClip(correctClip);

            if (successParticles != null)
                successParticles.Play();

            ShowTemporaryStatus("Box lift recorded");
        }
        else
        {
            PlayClip(duplicateClip);
            ShowTemporaryStatus("Box already counted");
        }

        UpdateCountText();
    }

    public void ResetSession()
    {
        StopStatusCoroutine();

        state.ResetSession();
        elapsedSeconds = 0f;

        StopParticles();

        if (resettableBoxes != null)
        {
            foreach (TrackableBox box in resettableBoxes)
            {
                if (box != null)
                    box.ResetToStart();
            }
        }

        RefreshUI("Press Start Session");
    }

    private void RefreshUI(string status)
    {
        UpdateCountText();
        UpdateTimeText();

        if (statusText != null)
            statusText.text = "Status: " + status;
    }

    private void UpdateCountText()
    {
        if (countText != null)
            countText.text = "Boxes lifted: " + state.Count;
    }

    private void UpdateTimeText()
    {
        if (timeText == null)
            return;

        int totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeText.text =
            $"Time elapsed: {minutes:00}:{seconds:00}";
    }

    private void ShowTemporaryStatus(string message)
    {
        StopStatusCoroutine();
        statusCoroutine = StartCoroutine(
            ShowStatusForTwoSeconds(message)
        );
    }

    private IEnumerator ShowStatusForTwoSeconds(string message)
    {
        if (statusText != null)
            statusText.text = "Status: " + message;

        yield return new WaitForSeconds(2f);

        statusCoroutine = null;

        string normalStatus = state.IsRunning
            ? "Session running"
            : "Press Start Session";

        if (statusText != null)
            statusText.text = "Status: " + normalStatus;
    }

    private void StopStatusCoroutine()
    {
        if (statusCoroutine == null)
            return;

        StopCoroutine(statusCoroutine);
        statusCoroutine = null;
    }

    private void StopParticles()
    {
        if (successParticles != null)
        {
            successParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}