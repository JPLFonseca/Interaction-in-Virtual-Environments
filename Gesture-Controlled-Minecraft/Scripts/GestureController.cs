using System.Collections.Generic;
using UnityEngine;


// Traudz os gestos das mãos em ações concretas no jogo
// Recebe do UDPReceiver e dá ordens
public class GestureController : MonoBehaviour
{
    public UDPReceiver receiver;

    
    [Header("Scripts to Control")]
    public FirstPersonMovement movementScript;
    public Interaction interactionScript;
    public FirstPersonLook lookScript;
    

    // This tracks the last message printed to the console to avoid spam
    private string lastPrintedMessage = "";


    // --- Private state to trigger actions only once per pinch ---
    private bool wasRightPinching = false;
    private bool wasLeftPinching = false;
    public float lookSensitivity = 1.0f;

    
    private void Start()
    {
    }

    void Update()
    {
        // --- Null checks ---
        if (receiver == null || movementScript == null || interactionScript == null || lookScript == null)
        {
            return;
        }

        // Obter o estado das mãos
        bool rightHandOnScreen = receiver.Hands.ContainsKey("Right");
        bool leftHandOnScreen = receiver.Hands.ContainsKey("Left");
        string currentMessage = "";

        // --- 1. Handle Movement ---
        // This logic is based on the number of hands detected.
        if (!rightHandOnScreen && !leftHandOnScreen)
        {
            currentMessage = "No hands on screen";
            movementScript.TriggerMoveForward();
        }
        else
        {
            // Stop if one OR both hands are on screen.
            movementScript.TriggerStop();
            if (rightHandOnScreen && leftHandOnScreen)
            {
                currentMessage = "Both hands on screen";
            }
        }

        // --- 2. Handle Looking ---
        // This logic is now separate to allow combined actions.
        float lookX = 0;
        float lookY = 0;

        // Horizontal look (left/right) still requires ONLY one hand.
        if (rightHandOnScreen && !leftHandOnScreen)
        {
            lookX = 1f; // Look right
        }
        else if (leftHandOnScreen && !rightHandOnScreen)
        {
            lookX = -1f; // Look left
        }

        
        // Vertical look (up/down) can happen WHENEVER the right hand is on screen.
        if (rightHandOnScreen)
        {
            string gesture = receiver.Hands["Right"].gesture;
            if (gesture == "up")
            {
                currentMessage = "Index finger up right";
                lookY = 1f; // Positive Y for looking up
            }
            else if (gesture == "down")
            {
                currentMessage = "Index finger down right";
                lookY = -1f; // Negative Y for looking down
            }
        }

        // Apply all look inputs to the look script
        lookScript.gestureLookInput = new Vector2(lookX * lookSensitivity, lookY * lookSensitivity);


        // --- 3. Handle Interaction ---
        // This logic remains the same.
        bool isRightPinching = rightHandOnScreen && receiver.Hands["Right"].gesture == "pinch";
        if (isRightPinching && !wasRightPinching)
        {
            currentMessage = "Pinch right";
            interactionScript.TriggerDestroy();
        }
        wasRightPinching = isRightPinching;

        bool isLeftPinching = leftHandOnScreen && receiver.Hands["Left"].gesture == "pinch";
        if (isLeftPinching && !wasLeftPinching)
        {
            currentMessage = "Pinch left";
            interactionScript.TriggerBuild();
        }
        wasLeftPinching = isLeftPinching;


        // --- 4. Print Status Messages ---
        // This logic remains the same.
        if (!string.IsNullOrEmpty(currentMessage) && currentMessage != lastPrintedMessage)
        {
            Debug.Log(currentMessage);
            lastPrintedMessage = currentMessage;
        }
    }
}