// Purpose: This script is responsible for handling the login process of the user. 
// It sends a GraphQL mutation request to verify the OTP provided by the user and 
// logs the user in if the OTP is correct.
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using TMPro;
public class LoginScript : MonoBehaviour
{

    public GameObject OTPInput;   // Input field for the OTP
    public GameObject menuCanvas;   // Canvas for the main menu
    public GameObject loginCanvas;  // Canvas for the login screen
    public GameObject submitBtn;  // Button to submit the OTP
    public Text errorMessage;   // Text field to display error messages
    public GraphQLHttpClient graphQLClient;  // GraphQL client for sending requests

    void Start()
    {
        // The client uses "http://localhost:4000/graphql" as the endpoint and NewtonsoftJsonSerializer for serialization.
        var graphQLC = new GraphQLHttpClient("http://localhost:4000/graphql", new NewtonsoftJsonSerializer());
        graphQLClient = graphQLC;

        // listeners for the input fields
        OTPInput.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate { handleSearchChange(); });
        submitBtn.GetComponent<Button>().onClick.AddListener(delegate { submitOTP(); });
    }

    // Function to handle the change in the input field
    public void handleSearchChange()
    {
        Debug.Log(OTPInput.GetComponent<TMP_InputField>().text);
    }

    // Function to submit the OTP
    public async void submitOTP()
    {
        // Log the current text from the OTP input field for debugging purposes.
        Debug.Log(OTPInput.GetComponent<TMP_InputField>().text.ToString());

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
            Variables = new { input = new { otp = OTPInput.GetComponent<TMP_InputField>().text } }
        };

        // Send the mutation request asynchronously and wait for the response.
        var queryResult = await graphQLClient.SendMutationAsync<JObject>(verifyOTP);
        var data = queryResult.Data;
        if (data != null)  // Success in retrieving Data
        {
            Debug.Log(data);
            string accessToken = (string)data["verifyOTP"]["accessToken"];
            PlayerPrefs.SetString("accessToken", accessToken);
        }
        else if (queryResult.Errors != null) // Failure to retrieve data
        {
            if ((string)queryResult.Errors[0].Extensions["code"] == "INTERNAL_SERVER_ERROR")
            {
                errorMessage.text = "Error Occured! Please try again";
            }
            else
            {
                Debug.Log(queryResult.Errors[0].Message);
                errorMessage.text = queryResult.Errors[0].Message;
            }
        }
    }
}
