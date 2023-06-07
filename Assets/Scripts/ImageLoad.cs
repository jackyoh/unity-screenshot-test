using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
using TMPro;

public class ImageLoad : MonoBehaviour {
    public TextMeshProUGUI message;

    public async void Start() {
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
        string localFilePath = Application.persistentDataPath + "/shake.png";
        
        var s3Client = new AmazonS3Client(credentialsIdentity.Credentials, RegionEndpoint.USEast1);
        TransferUtility utility = new TransferUtility(s3Client);
        TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest();
        request.BucketName = "amplify-unitytest-dev-164133-deployment";
        request.Key = "shake.png";
        request.FilePath = localFilePath;
        utility.Download(request);
        Debug.Log("Local File Path:" + localFilePath);

        /*GetObjectRequest request = new GetObjectRequest {
            BucketName = "amplify-unitytest-dev-164133-deployment",
            Key = "shake.png"
        };
        GetObjectResponse response1 = await s3Client.GetObjectAsync(request);        
        using (StreamReader reader = new StreamReader(response1.ResponseStream)) {
            using (var memstream = new MemoryStream()) {
                var buffer = new byte[512];
                var bytesRead = default(int);
                while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                        memstream.Write(buffer, 0, bytesRead);
                FileStream file = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                memstream.WriteTo(file);
                file.Close();
                memstream.Close();
            }
        }*/

        message.text = localFilePath;

        var rawData = System.IO.File.ReadAllBytes(localFilePath);
        Texture2D texture2D = new Texture2D(500, 500);
        texture2D.LoadImage(rawData);
        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(1f, 1f), 100f, 1, SpriteMeshType.FullRect);
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
