using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    public TMP_Dropdown resolutionDropdown;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Cursor Settings")]
    public bool showCursorWhenPaused = true;

    private Resolution[] resolutions;
    private List<Resolution> uniqueResolutions = new List<Resolution>();
    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetCursorState(false);

        SetupVolume();
        SetupResolutionDropdown();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    OpenPausePanel();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;
        SetCursorState(showCursorWhenPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SetCursorState(false);
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OpenPausePanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        SetCursorState(true);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetupVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = savedVolume;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = savedVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        resolutions = Screen.resolutions;
        uniqueResolutions.Clear();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        HashSet<string> added = new HashSet<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            if (!added.Contains(option))
            {
                added.Add(option);
                uniqueResolutions.Add(resolutions[i]);
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = uniqueResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, uniqueResolutions.Count - 1);

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        ApplySavedResolution();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void ApplySavedResolution()
    {
        if (uniqueResolutions.Count == 0)
            return;

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, uniqueResolutions.Count - 1);

        Resolution resolution = uniqueResolutions[savedIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (uniqueResolutions.Count == 0)
            return;

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, uniqueResolutions.Count - 1);

        Resolution resolution = uniqueResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}