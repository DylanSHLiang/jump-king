using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    public GameObject start;
    public GameObject settings;

    public Dropdown resolutionDropdown;

    public LayerMask UI;

    private List<Resolution> resolutions = new List<Resolution>();

    void Start()
    {
        GetResolutions();
    }

    private void GetResolutions()
    {
        List<Resolution> temp = Screen.resolutions.ToList();
        temp.Reverse();
        resolutionDropdown.ClearOptions();

        HashSet<string> options = new HashSet<string>();

        for (int i = 0; i < temp.Count; i++)
        {
            if (!options.Contains(temp[i].width + " x " + temp[i].height))
            {
                options.Add(temp[i].width + " x " + temp[i].height);
                resolutions.Add(temp[i]);
            }
        }

        resolutionDropdown.AddOptions(options.ToList());
        resolutionDropdown.value = 0;
        resolutionDropdown.RefreshShownValue();
    } 

    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Settings()
    {
        settings.SetActive(true);
        start.SetActive(false);
    }

    public void Pause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            settings.SetActive(false);
        } else
        {
            settings.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void BackToStart()
    {
        start.SetActive(true);
        settings.SetActive(false);
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("Start");
    }
}
