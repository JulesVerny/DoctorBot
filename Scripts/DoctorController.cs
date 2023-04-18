using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoctorController : MonoBehaviour
{
    // =========================================================================
    Animator TheAnimator;
    public DialogeController.CitzStatus TheCitizenState;
    public GameObject CharEngagementIndicator;
    private bool CharEngaged;
    // =========================================================================
    private void Awake()
    {
        TheAnimator = GetComponent<Animator>();
        TheCitizenState = DialogeController.CitzStatus.Idle; 

    }  // Awake 
    // =========================================================================
    void Start()
    {
        CharEngagementIndicator.SetActive(false);
        CharEngaged = false;
    } // Start
   // =========================================================================
    public void SetCurrentEngagement()
    {
        CharEngagementIndicator.SetActive(true);
        CharEngaged = true;
    } // SetCurrentEngagement
    public void ClearCurrentEngagement()
    {
        CharEngagementIndicator.SetActive(false);
        CharEngaged = false;
        SetStopIdle();
    } // ClearCurrentEngagement
    // ==========================================================================
    public void SetStopIdle()
    {
        //Debug.Log("[INFO]: Doctor Set Idle");
        TheAnimator.SetTrigger("IdleStop");
        TheCitizenState = DialogeController.CitzStatus.Idle;

    } // SetStopIdle
    // ==========================================================================
    public void SetTalking()
    {
        //Debug.Log("[INFO]: Doctor Set Talking");
        TheAnimator.SetTrigger("Talking");
        TheCitizenState = DialogeController.CitzStatus.Talking;

    } // SetTalking
    // ==========================================================================
    public void SetArguing()
    {
       
        TheAnimator.SetTrigger("Arguing");
        TheCitizenState = DialogeController.CitzStatus.Arguing;

    } // SetArguing
    // =========================================================================
    public void SetKicking()
    {
        TheAnimator.SetTrigger("Kicking");
        TheCitizenState = DialogeController.CitzStatus.Kicking;

    } // SetKicking
    // =========================================================================




    // =========================================================================

} // class DoctorController
