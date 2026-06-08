using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    public TMP_Dropdown resolutionDropdown;

    [Header("Scene Settings")]
    public string gameplaySceneName = "Gameplay";

    [Header("Cursor Settings")]
    public bool showCursor = true;

    private Resolution[] resolutions;
    private List<Resolution> uniqueResolutions = new List<Resolution>();

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // --- TAMBAHAN: Mainkan BGM Menu saat masuk Main Menu ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuBGM();
        }
        // -------------------------------------------------------

        SetCursorState(showCursor);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetupVolume();
        SetupResolutionDropdown();
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // --- TAMBAHAN: Ganti ke BGM Gameplay saat tombol Play ditekan ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayBGM();
        }
        // ----------------------------------------------------------------

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PlayerPrefs.Save();

        Application.Quit();
        Debug.Log("Game ditutup.");
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