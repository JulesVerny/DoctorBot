using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{

    public ToggleGroup SelectionToggleGroup;
    public static bool AutoMode;

    // =====================================================================
    // Start is called before the first frame update
    void Start()
    {
        AutoMode = false; 
    } // Start

    // ===========================================================================================
    public void UponStartButton()
    {

        Toggle TheSelectedToggle = SelectionToggleGroup.GetFirstActiveToggle();
        Debug.Log("[INFO]: Selected Toggle: " + TheSelectedToggle.name);

        if(TheSelectedToggle.name=="UserInteractionToggle") AutoMode = false;
        if (TheSelectedToggle.name == "AutoInteractionToggle") AutoMode = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);

    } // UponStartButton
      // ===========================================================================================
    void Update()
    {
        // Check if Quit Application
        if (Input.GetKey(KeyCode.Escape))
        {
            Debug.Log("[INFO]: Exit App by Escape Button");
            Application.Quit();
        }  // Escape App Check 
    } // Update
    // =========================================================================

} // IntroManager

