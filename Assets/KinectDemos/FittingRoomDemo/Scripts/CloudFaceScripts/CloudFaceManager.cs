﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Newtonsoft.Json.Serialization;
//using Newtonsoft.Json;
using System.Text;
using System;


public class CloudFaceManager : MonoBehaviour 
{
	[Tooltip("Subscription key for Face API. Go to https://www.microsoft.com/cognitive-services/en-us/face-api and push the 'Get started for free'-button to get new subscripiption keys.")]
	public string faceSubscriptionKey;

	[HideInInspector]
	public Face[] faces;  // the detected faces

	//private const string ServiceHost = "https://api.projectoxford.ai/face/v1.0";
	private const string ServiceHost = "https://westus.api.cognitive.microsoft.com/face/v1.0";
	private static CloudFaceManager instance = null;
	private bool isInitialized = false;


	void Awake()
	{
		instance = this;
	}

	void Start () 
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("Please set your face-subscription key.");
		}

		isInitialized = true;
	}

	/// <summary>
	/// Gets the CloudFaceManager instance.
	/// </summary>
	/// <value>The CloudFaceManager instance.</value>
	public static CloudFaceManager Instance
	{
		get
		{
			return instance;
		}
	}


	/// <summary>
	/// Determines whether the FaceManager is initialized.
	/// </summary>
	/// <returns><c>true</c> if the FaceManager is initialized; otherwise, <c>false</c>.</returns>
	public bool IsInitialized()
	{
		return isInitialized;
	}


	/// <summary>
	/// Detects the faces in the given image.
	/// </summary>
	/// <returns>List of detected faces.</returns>
	/// <param name="texImage">Image texture.</param>
	public IEnumerator DetectFaces(Texture2D texImage)
	{
		if (texImage != null) 
		{
			byte[] imageBytes = texImage.EncodeToJPG ();
			yield return DetectFaces (imageBytes);
		} 
		else 
		{
			yield return null;
		}
	}
	
	
	/// <summary>
	/// Detects the faces in the given image.
	/// </summary>
	/// <returns>List of detected faces.</returns>
	/// <param name="imageBytes">Image bytes.</param>
	public IEnumerator DetectFaces(byte[] imageBytes)
	{
		faces = null;

		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}

		string requestUrl = string.Format("{0}/detect?returnFaceId={1}&returnFaceLandmarks={2}&returnFaceAttributes={3}", 
											ServiceHost, true, false, "age,gender,smile,facialHair,glasses");
        //Debug.Log("Request: " + requestUrl);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		headers.Add("Content-Type", "application/octet-stream");
		headers.Add("Content-Length", imageBytes.Length.ToString());

		WWW www = new WWW(requestUrl, imageBytes, headers);
		yield return www;

        //Debug.Log("Response: " + www.text);

//		if (!string.IsNullOrEmpty(www.error)) 
//		{
//			throw new Exception(www.error + " - " + requestUrl);
//		}

		if(!CloudWebTools.IsErrorStatus(www))
		{
			//faces = JsonConvert.DeserializeObject<Face[]>(www.text, jsonSettings);
			string newJson = "{ \"faces\": " + www.text + "}";
			FacesCollection facesCollection = JsonUtility.FromJson<FacesCollection>(newJson);
            //Debug.Log("Faces-count: " + facesCollection.faces.Length);
            //if(facesCollection.faces.Length > 0)
            //    Debug.Log("Face0: " + facesCollection.faces[0].faceId + ", Gender: " + facesCollection.faces[0].faceAttributes.gender + ", Age: " + facesCollection.faces[0].faceAttributes.age);
			faces = facesCollection.faces;
        }
		else
		{
			ProcessFaceError(www);
		}
	}


	// processes the error status in response
	private void ProcessFaceError(WWW www)
	{
		//ClientError ex = JsonConvert.DeserializeObject<ClientError>(www.text);
		ClientError ex = JsonUtility.FromJson<ClientError>(www.text);
		
		if (ex.error != null && ex.error.code != null)
		{
			string sErrorMsg = !string.IsNullOrEmpty(ex.error.code) && ex.error.code != "Unspecified" ?
				ex.error.code + " - " + ex.error.message : ex.error.message;
			throw new System.Exception(sErrorMsg);
		}
		else
		{
			//ServiceError serviceEx = JsonConvert.DeserializeObject<ServiceError>(www.text);
			ServiceError serviceEx = JsonUtility.FromJson<ServiceError>(www.text);
			
			if (serviceEx != null && serviceEx.statusCode != null)
			{
				string sErrorMsg = !string.IsNullOrEmpty(serviceEx.statusCode) && serviceEx.statusCode != "Unspecified" ?
					serviceEx.statusCode + " - " + serviceEx.message : serviceEx.message;
				throw new System.Exception(sErrorMsg);
			}
			else
			{
				throw new System.Exception("Error " + CloudWebTools.GetStatusCode(www) + ": " + CloudWebTools.GetStatusMessage(www) + "; Url: " + www.url);
			}
		}
	}
	
	
//	private JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
//	{
//		DateFormatHandling = DateFormatHandling.IsoDateFormat,
//		NullValueHandling = NullValueHandling.Ignore,
//		ContractResolver = new CamelCasePropertyNamesContractResolver()
//	};


}
