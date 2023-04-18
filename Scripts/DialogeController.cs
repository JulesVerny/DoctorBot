using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

public class DialogeController : MonoBehaviour
{
    // =======================================================================
    public enum CitzStatus { Idle, Talking, Arguing, Kicking}

    // The Two Main Characters
    public DoctorController TheDoctorController;
    public TeacherController ThePatientController;
    private bool DoctorEngaged, PatientEngaged;

    // Main user Interface 
    public Button SubmitButton;
    public TMP_InputField NarratorInputTB;
    public TMP_Text DoctorResponseTB;
    public TMP_Text PatientResponseTB;

    public GameObject PatientsUIPanel;
    public GameObject MainUIPanel;

    // The Open_AI  API Interactions
    private OpenAIAPI TheOpenAI_api;
    private List<ChatMessage> DoctorsMessageHistory;
    private List<ChatMessage> PatientsMessageHistory;

    private string LastDoctorResponse;
    private string LastPatientResponse;


    // CountDowns
    private int DoctorTalkingCountDown;
    private int PatientTalkingCountDown;
    private int MessageChatInovationCount;
    private int MaxNumberofChatInvocations = 22;

    // Text To Speech
    private GCTextToSpeech TheTextToSpeechGenerator;
    private Voice[] PossibleVoices;
    private Voice TheCurrentVoice; 
    [SerializeField] AudioSource TheAudioPlayer;
    // =======================================================================
    private void Awake()
    {
        // Text To Speech
        TheTextToSpeechGenerator = GCTextToSpeech.Instance;
    }
    // =======================================================================
    void Start()
    {
        DoctorEngaged = false;
        PatientEngaged = false;
        TheDoctorController.ClearCurrentEngagement();
        ThePatientController.ClearCurrentEngagement();
        DoctorTalkingCountDown = 0;
        PatientTalkingCountDown = 0;
        MessageChatInovationCount = 0;

        // Instantiate the Open AI Interface
        TheOpenAI_api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPEN_AI_KEY", EnvironmentVariableTarget.Machine));

        // Confirm Which Mode Operating In
        Debug.Log("[INFO]: The Auto Play Mode is: " + IntroManager.AutoMode.ToString()); 

        if (IntroManager.AutoMode)
        {
            PatientsUIPanel.SetActive(true);
            MainUIPanel.SetActive(false);

            StartAutoConversationEngagement(); 
        }
        else
        {
            PatientsUIPanel.SetActive(false);
            MainUIPanel.SetActive(true);

            // Set Up the Narrative Conversation
            SubmitButton.onClick.AddListener(() => NarrativeSubmitButtonAction());
            StartNarrativeConversationEngagement();
        }

        // Text To Speech Events
        TheTextToSpeechGenerator.GetVoicesSuccessEvent += TheTextToSpeechGenerator_GetVoicesSuccessEvent;
        TheTextToSpeechGenerator.SynthesizeSuccessEvent += TheTextToSpeechGenerator_SynthesizeSuccessEvent;

        TheTextToSpeechGenerator.GetVoicesFailedEvent += TheTextToSpeechGenerator_GetVoicesFailedEvent;
        TheTextToSpeechGenerator.SynthesizeFailedEvent += TheTextToSpeechGenerator_SynthesizeFailedEvent;
    }  // Start

    // ================================================================================
    // The Text To Speech Voice Responses Events
    private void TheTextToSpeechGenerator_SynthesizeFailedEvent(string arg1, long arg2)
    {
        Debug.Log("[ERROR]: Speech To Synth Text Failure: " + arg1 + "  :  " + arg2.ToString()); 
    }
    // ====================================
    private void TheTextToSpeechGenerator_GetVoicesFailedEvent(string arg1, long arg2)
    {
        Debug.Log("[ERROR]: Speech To Voices Text Failure: " + arg1 + "  :  " + arg2.ToString());
    }
    // ====================================
    private void TheTextToSpeechGenerator_SynthesizeSuccessEvent(PostSynthesizeResponse response, long arg2)
    {
        Debug.Log("[INFO]: Speech Response ");

        TheAudioPlayer.clip = TheTextToSpeechGenerator.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
        TheAudioPlayer.Play();
    }
    // ====================================
    private void TheTextToSpeechGenerator_GetVoicesSuccessEvent(GetVoicesResponse response, long arg2)
    {
        PossibleVoices = response.voices;
    }
    // ====================================
    // =======================================================================
    private void StartNarrativeConversationEngagement()
    {
        // Initialise the Message History for a new Manual Conversation Session with the Doctor Only
        // So Only One message Stream
        DoctorsMessageHistory = new List<ChatMessage>();
        ChatMessage DoctorsIntroductoryMessage = new ChatMessage(ChatMessageRole.System, "You are a rather irritable and impatient Doctor, residing in a doctors waiting room. You are to provide short and succint responses to any patient queries. You are happy to discuss any patient back or muscle pains, illness general health conditions and mental health problems. However you get annoyed if requested to chat requests about small talk, the weather or any patient complaints about the service.");
        DoctorsMessageHistory.Add(DoctorsIntroductoryMessage);

        NarratorInputTB.text = "";

        string PatientIntroductoryString = "You have just entered the Doctors Waiting Area";

        DoctorResponseTB.text = PatientIntroductoryString;

        Debug.Log("[INFO]: Initial String: " + PatientIntroductoryString);

    } // StartNarrativeConversationEngagement
    // ===================================================================
    private void StartAutoConversationEngagement()
    {
        // Initialise the Message History for a new Auto Conversation Session between the OPatient and the Doctor
        // So Set up Two message Streams
        DoctorsMessageHistory = new List<ChatMessage>();
        PatientsMessageHistory = new List<ChatMessage>();
        
        // Set up the Doctors Context
        ChatMessage DoctorsIntroductoryMessage = new ChatMessage(ChatMessageRole.System, "You are a rather irritable and impatient Doctor, residing in a doctors waiting room. You are to provide short and succint responses to any patient queries. You are happy to discuss any patient back or muscle pains, illness and general health conditions and mental health problems. However you get annoyed if requested to chat requests about animals, the weather or any patient complaints.");
        DoctorsMessageHistory.Add(DoctorsIntroductoryMessage);

        ChatMessage PatientIntroductoryMessage = new ChatMessage(ChatMessageRole.System, "You are a rather aggressive acting patient, seeking advice on your own health. You are to provide short and succint responses to any queries. You are to respond with various symptoms of muscle pains in your lower limbs, and sometimes feel rather dizzy. You have tried paracentomol painkillers, but they had little effect. You consider yourself to be fit and run three times a week.  After a few responses you like to talk about the weather, losing your cat and making complaints about the doctors service.");
        PatientsMessageHistory.Add(PatientIntroductoryMessage);

        // Now Set up an Introductory Patient Message Into the Doctor
        ChatMessage InitialPatientMessage = new ChatMessage(ChatMessageRole.User, "Hello, is there a Doctor here?");

        // Invoke Doctors Chat
        PatientResponseTB.text = "Hello, Is there a Doctor here?";
        LastPatientResponse = "Hello, Is there a Doctor here?";
        // Set the Patients Voice
        VoiceConfig ThePatientVoiceConfig = new VoiceConfig() { gender = Enumerators.SsmlVoiceGender.MALE, languageCode = "en-AU", name = "en-AU-Wavenet-B" };
        // Compile a Synth Voice Request
        TheTextToSpeechGenerator.Synthesize("Hello, Is there a Doctor here?", ThePatientVoiceConfig, true, 1.0, 0.9, Constants.DEFAULT_SAMPLE_RATE, null);

        ThePatientController.SetCurrentEngagement();
        ThePatientController.SetTalking();
        PatientTalkingCountDown = 250; 

    } // StartAutoConversationEngagement
    // ===================================================================
    void NarrativeSubmitButtonAction()
    {
        // Confirm that the User has entered at least some Text, to avoid Calling with empty Queries
        if (NarratorInputTB.text.Length < 1) return;

        // Stop the Patient Annimating Upon Submit
        ThePatientController.ClearCurrentEngagement();
        ThePatientController.SetStopIdle();

        // First Disable the Submit button
        MainUIPanel.SetActive(false);
        GetDoctorChatResponse(NarratorInputTB.text);

    } // NarrativeSubmitButtonAction
    // =============================================================================
    public void NarrativeInputTBUpdated()
    {
        //Debug.Log("** TB Change: Length: " + NarratorInputTB.text.Length.ToString());
        if(NarratorInputTB.text.Length==1)
        {
            // Started Writing Some Narrative
            ThePatientController.SetCurrentEngagement();
            ThePatientController.SetTalking();
        }

    } // NarrativeInputTBUpdated
    // ==============================================================================
    private async void GetDoctorChatResponse(string DoctorsReqMessage)
    {
        TheDoctorController.SetCurrentEngagement();
        TheDoctorController.SetTalking();

        // Check Max Messages Count
        MessageChatInovationCount++;
        if(MessageChatInovationCount>= MaxNumberofChatInvocations)
        {
            DoctorResponseTB.text = "Sorry This Conversation is Over!";
            PatientResponseTB.text = "Sorry This Conversation is Over!";
            TheDoctorController.ClearCurrentEngagement();
            TheDoctorController.SetStopIdle();
            ThePatientController.ClearCurrentEngagement();
            ThePatientController.SetStopIdle();
            return;
        }  // Max Invocations

        // Now Retrieve the User Message from the Narrator Input
        ChatMessage DoctorUserMessage = new ChatMessage(ChatMessageRole.User, DoctorsReqMessage);
        //Trim any User Message to ensure not excessive
        if ((DoctorUserMessage.Content.Length > 200) && (!IntroManager.AutoMode))
        {
            DoctorUserMessage.Content = DoctorUserMessage.Content.Substring(0, 200);
        }
        Debug.Log(string.Format("[INFO]: User Role: {0}  : User Message: {1}", DoctorUserMessage.rawRole, DoctorUserMessage.Content));

        // Add the User Messgae into the Message History.
        DoctorsMessageHistory.Add(DoctorUserMessage);

        // Update the Text Field with the User message
        DoctorResponseTB.text = "Waiting Response....";

        // Set Up the Chat Request Object
        ChatRequest TheChatRequest = new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.2,
            /*MaxTokens = 75,*/
        Messages = DoctorsMessageHistory // Note we keep sending the entire Message History into the API 
        };

        // Now Make the Chat Call into Open API Interface
        var ChatResult = await TheOpenAI_api.Chat.CreateChatCompletionAsync(TheChatRequest);

        // Now process the Result after receiving the response
        ChatMessage DoctorResponseMessage = new ChatMessage();
        DoctorResponseMessage.Role = ChatResult.Choices[0].Message.Role;
        DoctorResponseMessage.Content = ChatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("[INFO]: Response Role: {0}  : Response Message: {1}", DoctorResponseMessage.rawRole, DoctorResponseMessage.Content));
        
        // Ensure that Add the Agent Reponse to the Message History
        DoctorsMessageHistory.Add(DoctorResponseMessage);

        // Now Update the Doctors Response UI
        DoctorResponseTB.text = DoctorResponseMessage.Content;
        LastDoctorResponse = DoctorResponseMessage.Content;

        // Now Set Up the Doctors Talk Animation and Chat Down Delay
        DoctorTalkingCountDown = 150 + 3*DoctorResponseMessage.Content.Length;       // Adjust the Wait Down to length of Response 

        // Now Set the Doctors Speech
        VoiceConfig TheDoctorsVoiceConfig = new VoiceConfig() { gender = Enumerators.SsmlVoiceGender.FEMALE, languageCode = "en-GB", name = "en-GB-Wavenet-A" };
        // Compile a Synth Voice Request
        TheTextToSpeechGenerator.Synthesize(LastDoctorResponse, TheDoctorsVoiceConfig, true, 1.0, 0.9, Constants.DEFAULT_SAMPLE_RATE, null);


    } //GetDoctorChatResponse
    // =======================================================================
    private async void GetPatientChatResponse(string PatientReqMessage)
    {
        ThePatientController.SetCurrentEngagement();
        ThePatientController.SetTalking();

        // Check Max Messages Count
        MessageChatInovationCount++;
        if (MessageChatInovationCount >= MaxNumberofChatInvocations)
        {
            DoctorResponseTB.text = "Sorry This Conversation is Over!";
            PatientResponseTB.text = "Sorry This Conversation is Over!";
            TheDoctorController.ClearCurrentEngagement();
            TheDoctorController.SetStopIdle();
            ThePatientController.ClearCurrentEngagement();
            ThePatientController.SetStopIdle();
            return;
        }

        // Now Retrieve the User Message from the Narrator Input
        ChatMessage PatientUserMessage = new ChatMessage(ChatMessageRole.User, PatientReqMessage);
        //Trim any User Message to ensure not excessive
        if ((PatientUserMessage.Content.Length > 200) && (!IntroManager.AutoMode))
        {
            PatientUserMessage.Content = PatientUserMessage.Content.Substring(0, 200);
        }
        Debug.Log(string.Format("[INFO]: User Role: {0}  : User Message: {1}", PatientUserMessage.rawRole, PatientUserMessage.Content));

        // Add the User Messgae into the Message History.
        PatientsMessageHistory.Add(PatientUserMessage);

        // Update the Text Field with the User message
        PatientResponseTB.text = "Waiting Response....";

        // Set Up the Chat Request Object
        ChatRequest TheChatRequest = new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.2,
            /*MaxTokens = 40,*/
            Messages = PatientsMessageHistory // Note we keep sending the entire Message History into the API 
        };

        // Now Make the Chat Call into Open API Interface
        var ChatResult = await TheOpenAI_api.Chat.CreateChatCompletionAsync(TheChatRequest);

        // Now process the Result after receiving the response
        ChatMessage PatientResponseMessage = new ChatMessage();
        PatientResponseMessage.Role = ChatResult.Choices[0].Message.Role;
        PatientResponseMessage.Content = ChatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("[INFO]: Response Role: {0}  : Response Message: {1}", PatientResponseMessage.rawRole, PatientResponseMessage.Content));

        // Ensure that Add the Agent Reponse to the Message History
        PatientsMessageHistory.Add(PatientResponseMessage);

        // Now Update the Doctors Response UI
        PatientResponseTB.text = PatientResponseMessage.Content;
        LastPatientResponse = PatientResponseMessage.Content;

        // Now Set Up the Patient Talk Animation and Chat Down Delay
        PatientTalkingCountDown = 150 + 3*PatientResponseMessage.Content.Length;        

        // Set the Patients Voice
        VoiceConfig ThePatientVoiceConfig = new VoiceConfig() { gender = Enumerators.SsmlVoiceGender.MALE, languageCode = "en-AU", name = "en-AU-Wavenet-B" };
        // Compile a Synth Voice Request
        TheTextToSpeechGenerator.Synthesize(LastPatientResponse, ThePatientVoiceConfig, true, 1.0, 0.9, Constants.DEFAULT_SAMPLE_RATE, null);

    } //GetPatientChatResponse
    // =======================================================================


    // =======================================================================
    private void ProvokeADoctorsResponse(string UserMessage)
    {
        GetDoctorChatResponse(UserMessage);
    } // ProvokeADoctorsResponse
    // =======================================================================
    private void ProvokeAPatientResponse(string UserMessage)
    {
        GetPatientChatResponse(UserMessage);
    } // ProvokePatientResponse
    // =======================================================================


    // =======================================================================
    // Debug User Interactions
    void Update()
    {
        // Check if Quit the Scene
        if (Input.GetKey(KeyCode.Escape))
        {
            Debug.Log("[INFO] Exit Scene by Escape Button");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }  // Escape or Quit Q Check 

        
    } // Update
    // =======================================================================
    private void FixedUpdate()
    {

        // Manage Count Downs, to enable short conversational and animations to play through

        // Check Doctor in Conversation
        if(DoctorTalkingCountDown>0)
        {
            DoctorTalkingCountDown--;
            if(DoctorTalkingCountDown<=0)
            {
                // Doctor Talking Has Completed
                TheDoctorController.ClearCurrentEngagement();
                TheDoctorController.SetStopIdle();

                if(IntroManager.AutoMode)
                {
                    // Provoke Patient message
                    ProvokeAPatientResponse(LastDoctorResponse); 
                }

                // If in Narration Mode:
                if (!IntroManager.AutoMode)
                {
                    //Clear the Narration Input Field  and Re enable the Submit Button
                    NarratorInputTB.text = "";
                    SubmitButton.enabled = true;
                    MainUIPanel.SetActive(true);
                }
            }
        } // Check Doctor in Conversation
        // ==============================

        // Check Patient in Conversation
        if ((PatientTalkingCountDown > 0)  && (IntroManager.AutoMode))
        {
            PatientTalkingCountDown--;
            if (PatientTalkingCountDown <= 0)
            {
                // Patient Talking Has Completed
                ThePatientController.ClearCurrentEngagement();
                ThePatientController.SetStopIdle();
                ProvokeADoctorsResponse(LastPatientResponse);
            }
        } // Check Doctor in Conversation

    } // FixedUpdate    
   
// =======================================================================
} // Dialog Controller
