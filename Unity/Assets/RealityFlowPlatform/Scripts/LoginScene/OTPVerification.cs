using UnityEngine;
using TMPro; // Add this for TMP_InputField
using UnityEngine.UI;

public class OTPVerification : MonoBehaviour
{
    public TMP_InputField otpInputField; // Change the type to TMP_InputField
    public Button submitButton;
    private string otpCode = "";
    private RealityFlowClient rfClient;

    void Start()
    {
        // Find the RealityFlowClient in the scene
        rfClient = FindObjectOfType<RealityFlowClient>();

        // Check if rfClient is found
        if (rfClient == null)
        {
            Debug.LogError("RealityFlowClient not found in the scene.");
            return;
        }

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
        submitButton.onClick.AddListener(SubmitOTP);
    }

    void AddDigit(int digit)
    {
        if (otpCode.Length < 4)
        {
            otpCode += digit.ToString();
            otpInputField.text = otpCode;
        }
    }

    public async void SubmitOTP()
    {
        // Log the current text from the OTP input field for debugging purposes.
        Debug.Log(otpInputField.text);

        // Create a new GraphQL mutation request to verify the OTP provided by the user.
        var verifyOTP = new GraphQLRequest
        {
            Query = @"
                   mutation VerifyOTP($input: VerifyOTPInput!) {
                        verifyOTP(input: $input) {
                            accessToken
                         }
                   }
            ",
            OperationName = "VerifyOTP",
            Variables = new { input = new { otp = otpInputField.text } }
        };

        // Send the mutation request asynchronously and wait for the response.
        var queryResult = await rfClient.SendQueryAsync(verifyOTP);
        var data = queryResult["data"];
        var errors = queryResult["errors"];
        if (data != null && errors == null)  // Success in retrieving Data
        {
            Debug.Log(data);
            string accessToken = (string)data["verifyOTP"]["accessToken"];
            PlayerPrefs.SetString("accessToken", accessToken);
        }
        else if (errors != null) // Failure to retrieve data
        {
            Debug.Log(errors[0]["message"]);
        }
    }

    public void ClearCode()
    {
        otpCode = "";
        otpInputField.text = otpCode;
    }
}