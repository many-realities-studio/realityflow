using UnityEngine;
using TMPro; // Add this for TMP_InputField
using UnityEngine.UI;
using Samples.Whisper;
using UnityEngine.Events;

public class OTPVerification : MonoBehaviour
{
    public GameObject projectDisplay;
    public TMP_InputField otpInputField; // Change the type to TMP_InputField
    public Button submitButton;
    private string otpCode = "";
    public UnityAction<string> onOTPSubmitted;


    void Start()
    {
        // Check if otpInputField and submitButton are assigned
        if (otpInputField == null || submitButton == null)
        {
            Debug.LogError("otpInputField or submitButton is not assigned.");
            return;
        }

        // Find all the buttons by their names and add listeners to them
        for (int i = 0; i <= 9; i++)
        {
            int number = i; // Local variable to capture the correct value in the closure
            Button button = GameObject.Find("n" + i + "_Button").GetComponent<Button>();
            button.onClick.AddListener(() => AddDigit(number));
        }

        // Add listener to the submit button
        submitButton.onClick.AddListener(() => onOTPSubmitted.Invoke(otpInputField.text));
    }

    void AddDigit(int digit)
    {
        if (otpCode.Length < 4)
        {
            otpCode += digit.ToString();
            otpInputField.text = otpCode;
        }
    }

    public void ClearCode()
    {
        otpCode = "";
        otpInputField.text = otpCode;
    }
}