using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;


using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;


public class TestCamera : MonoBehaviour {
    [SerializeField] private TilemapRenderer tilemapRenderer;
    [SerializeField] private RectTransform rectTransform;
    

    public async void Start() {
        Color originalBackground = Camera.main.backgroundColor;
        /*
        string persistentDataPath = Application.persistentDataPath;
        string temporaryCachePath = Application.temporaryCachePath;
        string filePath = "/home/user1/aaa/result2.png";
        string applicationDataPath = Application.dataPath;
        */
        string fileName = RandomString(5) + ".png";
        //string filePath = "/home/user1/aaa/" + fileName;
        string filePath = Application.persistentDataPath + "/" + fileName; 
        

        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        Camera screenshotCamera = GetComponent<Camera>();
        screenshotCamera.CopyFrom(Camera.main);

        screenshotCamera.targetTexture = rt;
        screenshotCamera.backgroundColor = Color.white;
        tilemapRenderer.enabled = false;
        screenshotCamera.Render();
        screenshotCamera.backgroundColor = originalBackground;
        tilemapRenderer.enabled = true;

        /*Debug.Log("Width:" + Screen.width / 2 + ", Height:" + Screen.height / 2);
        Debug.Log("Left:" + rectTransform.offsetMin.x);
        Debug.Log("Right:" + rectTransform.offsetMax.x);
        Debug.Log("Top:" + rectTransform.offsetMax.y);
        Debug.Log("Bottom:" + rectTransform.offsetMin.y);*/

        var width = Screen.width - (Mathf.Abs(rectTransform.offsetMin.x) + Mathf.Abs(rectTransform.offsetMax.x));
        var height = Screen.height - (Mathf.Abs(rectTransform.offsetMin.y) + Mathf.Abs(rectTransform.offsetMax.y));
        var x = rectTransform.offsetMin.x;
        var y = rectTransform.offsetMin.y;
        //int width = 450;
        //int height = 300;

        /*int width = Screen.width;
        int height = Screen.height;
        int x = 0;
        int y = 0;*/

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);        
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect((int)x, (int)y, width, height), 0, 0);
        texture.Apply();
        byte[] bytes = texture.EncodeToPNG();


        Texture2D sourceTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        sourceTexture.LoadImage(bytes);

        Color[] pixels = sourceTexture.GetPixels();
        for (int i = 0 ; i < pixels.Length ; i++) {
            if (pixels[i] == Color.white) {
                pixels[i].a = 0f;
            }
        }
        sourceTexture.SetPixels(pixels);
        sourceTexture.Apply();
        bytes = sourceTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;
        screenshotCamera.targetTexture = null;

        //-------------[Upload data to S3 storage]----------------
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        IAmazonCognitoIdentityProvider cognitoService;
        cognitoService = new AmazonCognitoIdentityProviderClient(
            new AnonymousAWSCredentials(), RegionEndpoint.USEast1
        );
        string username = "user100";
        string password = "9775630345";
        var authParameters = new Dictionary<string, string>();
        authParameters.Add("USERNAME", username);
        authParameters.Add("PASSWORD", password);

        var authRequest = new InitiateAuthRequest {
            ClientId = "76dq8d06b7aio2v74trg8qn0uq",
            AuthParameters = authParameters,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
        };

        InitiateAuthResponse response = await cognitoService.InitiateAuthAsync(authRequest);
        var authResult = response.AuthenticationResult;
        string idToken = authResult.IdToken;

        CognitoAWSCredentials credentials = 
            new CognitoAWSCredentials("us-east-1:176365b4-d93d-4589-a8a2-68f41ed6a31d", RegionEndpoint.USEast1);
        credentials.AddLogin("cognito-idp.us-east-1.amazonaws.com/us-east-1_aumofL5vx", idToken);

        AmazonCognitoIdentityClient cli = 
            new AmazonCognitoIdentityClient(credentials, RegionEndpoint.USEast1);
        var req = new Amazon.CognitoIdentity.Model.GetIdRequest();
        req.Logins.Add("cognito-idp.us-east-1.amazonaws.com/us-east-1_aumofL5vx", idToken);
        req.IdentityPoolId = "us-east-1:176365b4-d93d-4589-a8a2-68f41ed6a31d";

        GetIdResponse getIdResponse = await cli.GetIdAsync(req);
        var getCredentialReq = new Amazon.CognitoIdentity.Model.GetCredentialsForIdentityRequest();
        getCredentialReq.IdentityId = getIdResponse.IdentityId;
        getCredentialReq.Logins.Add("cognito-idp.us-east-1.amazonaws.com/us-east-1_aumofL5vx", idToken);
        var credentialsIdentity = await cli.GetCredentialsForIdentityAsync(getCredentialReq);
        
        // List file info from the s3
        /*var s3Client = new AmazonS3Client(credentialsIdentity.Credentials, RegionEndpoint.USEast1);
        var objects = (await s3Client.ListObjectsAsync("amplify-unitytest-dev-164133-deployment/")).S3Objects;
        string files = "";
        foreach (var file in objects) {
            files = files + file.Key + "\n";
        }
        Debug.Log(files);*/
        
        // Upload png file to s3
        var s3Client = new AmazonS3Client(credentialsIdentity.Credentials, RegionEndpoint.USEast1);
        TransferUtility utility = new TransferUtility(s3Client);
        TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
        request.BucketName = "amplify-unitytest-dev-164133-deployment";
        request.Key = fileName;
        request.FilePath = filePath;
        
        utility.Upload(request);
        //-------------[Upload data to S3 storage]----------------
    }

    private string RandomString(int length) {
        System.Random random = new System.Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    } 

}
