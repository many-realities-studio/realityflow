using UnityEngine;
using TMPro;

// NOTE: OTPVerification is the updated version of this script. 
public class LoginScript : MonoBehaviour
{
    public TMP_InputField OTPInput; // Input field for the OTP
    //public Button submitBtn;        // Button to submit the OTP
    public RealityFlowClient rfClient; // RealityFlow client for sending requests

    void Start()
    {
        // The client uses "http://localhost:4000/graphql" as the endpoint and NewtonsoftJsonSerializer for serialization.
        rfClient = FindObjectOfType<RealityFlowClient>();

        // Check if rfClient is found
        if (rfClient == null)
        {
            Debug.LogError("RealityFlowClient not found in the scene.");
            return;
        }

        // Check if OTPInput and submitBtn are assigned
        /*if (OTPInput == null || submitBtn == null)
        {
            Debug.LogError("OTPInput or submitBtn is not assigned.");
            return;
        }

        // Listeners for the input fields
        OTPInput.onValueChanged.AddListener(handleSearchChange);
        submitBtn.onClick.AddListener(submitOTP);*/
    }

    // Function to handle the change in the input field
    public void handleSearchChange(string value)
    {
        Debug.Log(value);
    }

    // Function to submit the OTP
    public async void submitOTP()
    {
        // Log the current text from the OTP input field for debugging purposes.
        Debug.Log(OTPInput.text);

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
            Variables = new { input = new { otp = OTPInput.text } }
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
}