// This is from "Resolution & Display Type | Unity | Settings - Part 1" by HamJoyGames.
// The assets are not exactly the same (we just wanted to demonstrate functionality).
// Controls the video settings (resolution and display type)
// NOTE: you can only fully test this AFTER BUILDING YOUR PROJECT!

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    // Buttons
    [SerializeField] private Button btnExit;

    // Dropdowns
    [SerializeField] private Dropdown ddVideoResolutions;
    [SerializeField] private Dropdown ddDisplay;

    // Pop Up
    [SerializeField] private GameObject resPopUp;
    [SerializeField] private Button btnAccept;
    [SerializeField] private Button btnRevert;
    [SerializeField] private int maximumPopUpTimer = 10;
    [SerializeField] private Text txtCountDown;

    // Private variables
    private List<Resolution> storeResolutions;
    private int prevDisplayTypeIndex;
    private int prevResolutionIndex;
    private FullScreenMode screenMode;

    #region Resolutions
    // Converts the resolution into a readable string
    string ResolutionToString(Resolution screenRes)
    {
        return screenRes.width + " x " + screenRes.height + " @ " + screenRes.refreshRate + " Hz";
    }

    // Find all resolutions on the user's computer
    void FindResolutions()
    {
        storeResolutions = new List<Resolution>();
        Resolution[] resolutions = Screen.resolutions;
        Array.Reverse(resolutions);
        for (int i = 0; i < resolutions.Length; i++)
        {
            ddVideoResolutions.options.Add(new Dropdown.OptionData(ResolutionToString(resolutions[i])));

            // Known bug: this does not take into account the refresh rate.
            // You'll get the right resolution but not the right refresh rate that you left off. (fixed with binary saving)
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                ddVideoResolutions.value = i;

            storeResolutions.Add(resolutions[i]);
        }
        ddVideoResolutions.RefreshShownValue();
    }

    // Applies the new resolution
    void SetResolution(int index)
    {
        Screen.SetResolution(storeResolutions[index].width, storeResolutions[index].height, screenMode);
        ddVideoResolutions.value = index;
        ddVideoResolutions.RefreshShownValue();
    }
    #endregion

    #region Display Type
    // Determines what display type that we should use
    void DisplayType(string mode)
    {
        // Known bug: If you ever lose focus of the game (alt tabing, task manager) while in exclusive full screen and close the game (end task, close window, etc),
        // the fullscreen mode registry will save it as a value of 3 (windowed) instead of 0 (exclusive full screen).
        // Therefore, when you reopen the game, it returns to windowed mode.
        // If you leave the game through the in game button, this doesn't happen.
        // I do not know why this interaction occurs. This is partially solved with binary file saving.
        if (mode == "ExclusiveFullScreen")
        {
            ddDisplay.value = 0;
            screenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if (mode == "Windowed")
        {
            ddDisplay.value = 1;
            screenMode = FullScreenMode.Windowed;
        }
        else
        {
            ddDisplay.value = 2;
            screenMode = FullScreenMode.FullScreenWindow;
        }
        
        Screen.SetResolution(Screen.width, Screen.height, screenMode);
        ddDisplay.RefreshShownValue();
    }

    // Applies the new display type
    void SetDisplay(int index)
    {
        DisplayType(ddDisplay.options[index].text);
        StartCoroutine("SetDisplayAtEnd");
    }

    // Calls Screen.SetResolution at the next fixedupdate call
    IEnumerator SetDisplayAtEnd()
    {
        yield return new WaitForFixedUpdate();
        Screen.SetResolution(Screen.width, Screen.height, screenMode);
    }
    #endregion

    #region Pop Up
    // When they choose either dropdown, assign the correct listeners
    void PopUpHandler(int index, Dropdown dropdown)
    {
        if (!resPopUp.activeSelf)
        {
            resPopUp.SetActive(true);
            btnAccept.onClick.RemoveAllListeners();
            btnRevert.onClick.RemoveAllListeners();
            if (dropdown == ddVideoResolutions)
            {
                SetResolution(index);
                btnAccept.onClick.AddListener(delegate { Apply(dropdown, index); });
                btnRevert.onClick.AddListener(delegate { Revert(dropdown, prevResolutionIndex); });
            }
            else
            {
                SetDisplay(index);
                btnAccept.onClick.AddListener(delegate { Apply(dropdown, index); });
                btnRevert.onClick.AddListener(delegate { Revert(dropdown, prevDisplayTypeIndex); });
            }

            StartCoroutine("Timer", dropdown);
        }
    }

    // Assign correct values to the previous indexes
    void Apply(Dropdown drop, int newIndex)
    {
        if (drop == ddVideoResolutions)
            prevResolutionIndex = newIndex;
        else
            prevDisplayTypeIndex = newIndex;

        ClosePopUp();
    }

    // Reverts back to the previous resolution or display type
    void Revert(Dropdown drop, int index)
    {
        if (drop == ddVideoResolutions)
            SetResolution(index);
        else
            SetDisplay(index);

        ClosePopUp();
    }

    // Close popups
    void ClosePopUp()
    {
        resPopUp.SetActive(false);
        StopCoroutine("Timer");
    }

    // Popup timer
    IEnumerator Timer(Dropdown dropdown)
    {
        int currentTimer = maximumPopUpTimer;
        while (currentTimer >= 0)
        {
            txtCountDown.text = currentTimer.ToString();
            yield return new WaitForSeconds(1);
            currentTimer--;

            if (currentTimer < 0)
            {
                if (dropdown == ddVideoResolutions)
                    Revert(dropdown, prevResolutionIndex);
                else
                    Revert(dropdown, prevDisplayTypeIndex);
            }
        }
    }
    #endregion

    #region Start Functions
    // Run these functions at start
    void Initialize()
    {
        FindResolutions();
        Debug.Log("Initialize: " + Screen.fullScreenMode.ToString());
        DisplayType(Screen.fullScreenMode.ToString());

        prevDisplayTypeIndex = ddDisplay.value;
        prevResolutionIndex = ddVideoResolutions.value;

        btnExit.onClick.AddListener(delegate { Application.Quit(); });
    }

    // Assigning button listeners
    void ButtonEvents()
    {
        ddVideoResolutions.onValueChanged.AddListener(delegate { PopUpHandler(ddVideoResolutions.value, ddVideoResolutions); });
        ddDisplay.onValueChanged.AddListener(delegate { PopUpHandler(ddDisplay.value, ddDisplay); });
    }
    #endregion

    void Start()
    {
        Initialize();
        ButtonEvents();
    }
}
