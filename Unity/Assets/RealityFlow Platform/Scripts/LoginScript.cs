using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
public class LoginScript : MonoBehaviour
{

    public GameObject OTPInput;
    public GameObject menuCanvas;
    public GameObject loginCanvas;
    public GameObject submitBtn;
    public Text errorMessage;
    public GraphQLHttpClient graphQLClient;

    // Start is called before the first frame update
    void Start()
    {
        var graphQLC = new GraphQLHttpClient("http://localhost:4000/graphql", new NewtonsoftJsonSerializer());
        graphQLClient = graphQLC;
        OTPInput.GetComponent<InputField>().onValueChanged.AddListener(delegate { handleSearchChange(); });
        submitBtn.GetComponent<Button>().onClick.AddListener(delegate { submitOTP(); });
        

    }
    public void readStringInput(string code)
    {
        submitOTP();
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void handleSearchChange()
    {
        Debug.Log(OTPInput.GetComponent<InputField>().text);
    }
    public async void submitOTP()

    {
        Debug.Log(OTPInput.GetComponent<InputField>().text.ToString());
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
            Variables = new { input = new { otp = OTPInput.GetComponent<InputField>().text } }
        };
        var queryResult = await graphQLClient.SendMutationAsync<JObject>(verifyOTP);
        var data = queryResult.Data;
        if (data != null)
        {
            Debug.Log(data);
            string accessToken = (string)data["verifyOTP"]["accessToken"];
            // PlayerPrefs.SetString("userId", (string)data["verifyOTP"]["OTPVerification"]["data"]["userId"]);
            PlayerPrefs.SetString("accessToken", accessToken);
            // loginCanvas.SetActive(false);
            // menuCanvas.SetActive(true);
        } else if (queryResult.Errors != null)
        {
            if ((string)queryResult.Errors[0].Extensions["code"] == "INTERNAL_SERVER_ERROR")
            {
                errorMessage.text = "Error Occured! Please try again";
            } else
            {
                Debug.Log(queryResult.Errors[0].Message);
                errorMessage.text = queryResult.Errors[0].Message;
            }
        }
       
    }

}
