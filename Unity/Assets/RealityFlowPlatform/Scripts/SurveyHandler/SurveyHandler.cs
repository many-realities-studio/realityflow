using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurveyHandler : MonoBehaviour
{
    private static SurveyHandler instance;
    public static SurveyHandler Instance { get { return instance; } }

    public GameObject MainPane;
    public GameObject DataPane;
    public GameObject SurveyPrefab;
    public GameObject QuestionPrefab;

    private RealityFlowClient rfClient;

    private JArray surveys;
    public JArray Surveys { get; set; }



    public void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        rfClient = RealityFlowClient.Find(this);
    }

    public void ShowSurvey(string projectId)
    {
        // Get survey info
        var getProjectData = new GraphQLRequest {
            Query = @"
                query GetProjectById($getProjectByIdId: String) {
                    getProjectById(id: $getProjectByIdId) {
                        preSurveys {
                            id
                            survey {
                                title
                                questions {id
                                text
                                }
                            }
                        }
                        postSurveys {
                        id
                        survey {
                            title
                                questions {
                                    id
                                    text
                                }
                            }
                        }
                    }
                }
                ",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = projectId }
        };
        var projectData = rfClient.SendQueryAsync(getProjectData);
        if (projectData == null)
            return;
        var surveyData = projectData["data"]["getProjectById"]["preSurveys"];
        if(surveyData == null)
            return;
        surveys = (JArray)surveyData;

        // Clear form
        foreach (Transform childTransform in DataPane.transform)
            Destroy(childTransform.gameObject);

        // Fill form
        foreach (JObject instance in surveys) {
            JToken survey = instance["survey"];
            
            var surveyObject = Instantiate(SurveyPrefab, DataPane.transform);
            surveyObject.GetComponent<TMP_Text>().text = (string)survey["title"];  
            foreach (JObject question in survey["questions"]) {
                var questionObject = Instantiate(QuestionPrefab, DataPane.transform);
                questionObject.transform.GetChild(0).GetComponent<TMP_Text>().text = (string)question["text"];
            }
        }


        MainPane.SetActive(true);
    }

    public void SubmitSurvey() {
        MainPane.SetActive(false);

        var surveyUpload = new GraphQLRequest {
            Query = @"
                mutation SurveyUpload($inputs: [CreateSurveyResponseInput]!) {
                    uploadSurveyResponses(input: $inputs) {
                        success
                    }
                }
                ",
            OperationName = "SurveyUpload"
        };


        JArray submissions = new JArray();
        
        int childIndex = 0;
        foreach (JObject instance in surveys) {
            Transform surveyTransform = DataPane.transform.GetChild(childIndex++);

            JObject submission = new JObject();
            submission.Add("instanceId", instance["id"]);
            JArray submissionQuestions = new JArray();

            JToken survey = instance["survey"];
            foreach (JObject question in survey["questions"]) {
                Transform questionTransform = DataPane.transform.GetChild(childIndex++);
                Slider slider = questionTransform.GetChild(1).gameObject.GetComponent<Slider>();

                JObject submissionQuestion = new JObject();
                submissionQuestion.Add("questionId", question["id"]);
                submissionQuestion.Add("answer", (int)slider.value);
                submissionQuestions.Add(submissionQuestion);
            }

            submission.Add("questionResponses", submissionQuestions);
            submissions.Add(submission);
        }
        
        surveyUpload.Variables = new { inputs = submissions };
        var projectData = rfClient.SendQueryAsync(surveyUpload);
    }
}
