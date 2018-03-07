using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;


/// <summary>
/// This interface needs to be implemented by all speech-recognition listeners
/// </summary>
public interface SpeechRecognitionInterface
{
	/// <summary>
	/// Invoked when speech phrase gets recognized.
	/// </summary>
	/// <returns><c>true</c>, if the recognized phrase has to be cleared, <c>false</c> otherwise.</returns>
	/// <param name="phraseTag">The phrase tag.</param>
	/// <param name="condidence">Recognized with condidence (0-1).</param>
	bool SpeechPhraseRecognized(string phraseTag, float condidence);
}

/// <summary>
/// Speech manager is the component that manages the Kinect speech recognition.
/// </summary>
public class SpeechManager : MonoBehaviour 
{
	[Tooltip("File name of the grammar file, used by the speech recognizer. The file will be copied from Resources, if it does not exist.")]
	public string grammarFileName = "SpeechGrammar.grxml";

	[Tooltip("Whether the grammar is dynamic or static. Dynamic grammars allow adding phrases at run-time.")]
	public bool dynamicGrammar = false;
	
	[Tooltip("Code of the language, used by the speech recognizer. Default is English (1033).")]
	public int languageCode = 1033;

	[Tooltip("Minimum confidence required, to consider a phrase as recognized. Confidence varies between 0.0 and 1.0.")]
	public float requiredConfidence = 0f;

	[Tooltip("List of the speech recognition listeners in the scene. If the list is empty, the available gesture listeners will be detected at the scene start up.")]
	public List<MonoBehaviour> speechRecognitionListeners;

	[Tooltip("GUI-Text to display the speech-manager debug messages.")]
	public GUIText debugText;

	// Is currently listening
	private bool isListening;
	
	// Current phrase recognized
	private bool isPhraseRecognized;
	private string phraseTagRecognized;
	private float phraseConfidence;
	
	// primary sensor data structure
	private KinectInterop.SensorData sensorData = null;
	
	// Bool to keep track of whether Kinect and SAPI have been initialized
	private bool sapiInitialized = false;
	
	// The single instance of SpeechManager
	private static SpeechManager instance;
	
	
	/// <summary>
	/// Gets the single SpeechManager instance.
	/// </summary>
	/// <value>The SpeechManager instance.</value>
    public static SpeechManager Instance
    {
        get
        {
            return instance;
        }
    }
	
	/// <summary>
	/// Determines whether SAPI (Speech API) was successfully initialized.
	/// </summary>
	/// <returns><c>true</c> if SAPI was successfully initialized; otherwise, <c>false</c>.</returns>
	public bool IsSapiInitialized()
	{
		return sapiInitialized;
	}

	/// <summary>
	/// Adds a phrase to the from-rule of dynamic grammar. If the to-rule is empty, this means end of the phrase recognition.
	/// </summary>
	/// <returns><c>true</c> if the phrase was successfully added to the grammar; otherwise, <c>false</c>.</returns>
	/// <param name="fromRule">From-rule name.</param>
	/// <param name="toRule">To-rule name or empty string.</param>
	/// <param name="phrase">The dynamic phrase.</param>
	/// <param name="bClearRulePhrases">If set to <c>true</c> clears current rule phrases before adding this one.</param>
	/// <param name="bCommitGrammar">If set to <c>true</c> commits dynamic grammar changes.</param>
	public bool AddGrammarPhrase(string fromRule, string toRule, string phrase, bool bClearRulePhrases, bool bCommitGrammar)
	{
		if(sapiInitialized)
		{
			int hr = sensorData.sensorInterface.AddGrammarPhrase(fromRule, toRule, phrase, bClearRulePhrases, bCommitGrammar);
			return (hr == 0);
		}

		return false;
	}
	
	/// <summary>
	/// Determines whether the speech recogizer is in listening-state.
	/// </summary>
	/// <returns><c>true</c> if the speech recogizer is in listening-state; otherwise, <c>false</c>.</returns>
	public bool IsListening()
	{
		return isListening;
	}
	
	/// <summary>
	/// Determines whether the speech recognizer has recognized a phrase.
	/// </summary>
	/// <returns><c>true</c> if the speech recognizer has recognized a phrase; otherwise, <c>false</c>.</returns>
	public bool IsPhraseRecognized()
	{
		return isPhraseRecognized;
	}

	/// <summary>
	/// Gets the confidence of the currently recognized phrase, in range [0, 1].
	/// </summary>
	/// <returns>The phrase confidence.</returns>
	public float GetPhraseConfidence()
	{
		return phraseConfidence;
	}
	
	/// <summary>
	/// Gets the tag of the recognized phrase.
	/// </summary>
	/// <returns>The tag of the recognized phrase.</returns>
	public string GetPhraseTagRecognized()
	{
		return phraseTagRecognized;
	}
	
	/// <summary>
	/// Clears the recognized phrase.
	/// </summary>
	public void ClearPhraseRecognized()
	{
		isPhraseRecognized = false;
		phraseTagRecognized = String.Empty;
		phraseConfidence = 0f;
	}


	// gets speech recognition data as csv line
	public string GetSpeechDataAsCsv(char delimiter)
	{
		if (!sapiInitialized)
			return string.Empty;

		// create the output string
		StringBuilder sbBuf = new StringBuilder();
		sbBuf.Append("sr").Append(delimiter);

		if(isPhraseRecognized)
		{
			sbBuf.Append(1).Append(delimiter);
			sbBuf.Append(phraseTagRecognized).Append(delimiter);
			sbBuf.AppendFormat("{0:F3}", phraseConfidence).Append(delimiter);

			//Debug.Log(phraseTagRecognized + ", confidence: " + phraseConfidence);
		}
		else
		{
			sbBuf.Append(0).Append(delimiter);
		}

		// remove the last delimiter
		if(sbBuf.Length > 0 && sbBuf[sbBuf.Length - 1] == delimiter)
		{
			sbBuf.Remove(sbBuf.Length - 1, 1);
		}

		return sbBuf.ToString();
	}

	// sets speech recognition data from a csv line
	public bool SetSpeechDataFromCsv(string sCsvLine, char[] delimiters)
	{
		if(sCsvLine.Length == 0)
			return false;

		// split the csv line in parts
		string[] alCsvParts = sCsvLine.Split(delimiters);

		if(alCsvParts.Length < 1 || alCsvParts[0] != "sr")
			return false;

		int iIndex = 1;
		int iLength = alCsvParts.Length;

		if (iLength < (iIndex + 1))
			return false;

		// whether there is recognized phrase or not
		int phraseRecognized = 0;
		int.TryParse(alCsvParts[iIndex], out phraseRecognized);
		iIndex++;

		if (phraseRecognized != 0 && iLength >= (iIndex + 2)) 
		{
			// get the recognized phrase
			isPhraseRecognized = true;
			phraseTagRecognized = alCsvParts[iIndex];
			float.TryParse(alCsvParts[iIndex + 1], out phraseConfidence);
		}
//		else
//		{
//			// no phrase recognized
//			isPhraseRecognized = false;
//			phraseTagRecognized = String.Empty;
//			phraseConfidence = 0f;
//		}

		return true;
	}


	//----------------------------------- end of public functions --------------------------------------//


	void Awake()
	{
		instance = this;
	}


	void Start() 
	{
		try 
		{
			// get sensor data
			KinectManager kinectManager = KinectManager.Instance;
			if(kinectManager && kinectManager.IsInitialized())
			{
				sensorData = kinectManager.GetSensorData();
			}
			
			if(sensorData == null || sensorData.sensorInterface == null)
			{
				throw new Exception("Speech recognition cannot be started, because KinectManager is missing or not initialized.");
			}
			
			if(debugText != null)
			{
				debugText.text = "Please, wait...";
			}
			
			// ensure the needed dlls are in place and speech recognition is available for this interface
			bool bNeedRestart = false;
			if(sensorData.sensorInterface.IsSpeechRecognitionAvailable(ref bNeedRestart))
			{
				if(bNeedRestart)
				{
					KinectInterop.RestartLevel(gameObject, "SM");
					return;
				}
			}
			else
			{
				string sInterfaceName = sensorData.sensorInterface.GetType().Name;
				throw new Exception(sInterfaceName + ": Speech recognition is not supported!");
			}
			
			// Initialize the speech recognizer
			string sCriteria = String.Format("Language={0:X};Kinect=True", languageCode);
			int rc = sensorData.sensorInterface.InitSpeechRecognition(sCriteria, true, false);
	        if (rc < 0)
	        {
				string sErrorMessage = (new SpeechErrorHandler()).GetSapiErrorMessage(rc);
				throw new Exception(String.Format("Error initializing Kinect/SAPI: " + sErrorMessage));
	        }
			
			if(requiredConfidence > 0)
			{
				sensorData.sensorInterface.SetSpeechConfidence(requiredConfidence);
			}
			
			if(grammarFileName != string.Empty)
			{
				// copy the grammar file from Resources, if available
				//if(!File.Exists(grammarFileName))
				{
					TextAsset textRes = Resources.Load(grammarFileName, typeof(TextAsset)) as TextAsset;
					
					if(textRes != null)
					{
						string sResText = textRes.text;

#if !NETFX_CORE
						File.WriteAllText(grammarFileName, sResText);
#else
						System.Threading.Tasks.Task task = null;

//						UnityEngine.WSA.Application.InvokeOnUIThread(() =>
//						{
							task = CopyGrammarFileToStorageAsync(grammarFileName, sResText);
//						}, true);
						
						while (task != null && !task.IsCompleted && !task.IsFaulted)
						{
							task.Wait(100);
						}

						if(task != null)
						{
							if(task == null)
								throw new Exception("Could not create task for CopyGrammarFileToStorageAsync()");
							else if(task.IsFaulted)
								throw task.Exception;
						}
#endif
					}
					else
					{
						throw new Exception("Couldn't find grammar resource: " + grammarFileName + ".txt");
					}
				}

				// load the grammar file
				rc = sensorData.sensorInterface.LoadSpeechGrammar(grammarFileName, (short)languageCode, dynamicGrammar);
		        if (rc < 0)
		        {
					string sErrorMessage = (new SpeechErrorHandler()).GetSapiErrorMessage(rc);
					throw new Exception("Error loading grammar file " + grammarFileName + ": " + sErrorMessage);
		        }

//				// test dynamic grammar phrases
//				AddGrammarPhrase("addressBook", string.Empty, "Nancy Anderson", true, false);
//				AddGrammarPhrase("addressBook", string.Empty, "Cindy White", false, false);
//				AddGrammarPhrase("addressBook", string.Empty, "Oliver Lee", false, false);
//				AddGrammarPhrase("addressBook", string.Empty, "Alan Brewer", false, false);
//				AddGrammarPhrase("addressBook", string.Empty, "April Reagan", false, true);
			}
			
			sapiInitialized = true;
			
			//DontDestroyOnLoad(gameObject);

			if(debugText != null)
			{
				debugText.text = "Speech recognizer is ready.";
			}

			// try to automatically detect the available speech recognition listeners in the scene
			if(speechRecognitionListeners.Count == 0)
			{
				MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];

				foreach(MonoBehaviour monoScript in monoScripts)
				{
					if((monoScript is SpeechRecognitionInterface) && monoScript.enabled)
					{
						speechRecognitionListeners.Add(monoScript);
					}
				}
			}

		} 
		catch(DllNotFoundException ex)
		{
			Debug.LogError(ex.ToString());
			if(debugText != null)
				debugText.text = "Please check the Kinect and SAPI installations.";
		}
		catch (Exception ex) 
		{
			Debug.LogError(ex.ToString());
			if(debugText != null)
				debugText.text = ex.Message;
		}
	}

#if NETFX_CORE
	private async System.Threading.Tasks.Task CopyGrammarFileToStorageAsync(string grammarFileName, string grammarContent)
	{
		Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
		Windows.Storage.StorageFile grammarFile = await storageFolder.CreateFileAsync(grammarFileName,
			Windows.Storage.CreationCollisionOption.ReplaceExisting);
		
		await Windows.Storage.FileIO.WriteTextAsync(grammarFile, grammarContent);
	}
#endif

	void OnDestroy()
	{
		if(sapiInitialized && sensorData != null && sensorData.sensorInterface != null)
		{
			// finish speech recognition
			sensorData.sensorInterface.FinishSpeechRecognition();
		}
		
		sapiInitialized = false;
		instance = null;
	}
	
	void Update () 
	{
		// start Kinect speech recognizer as needed
//		if(!sapiInitialized)
//		{
//			StartRecognizer();
//			
//			if(!sapiInitialized)
//			{
//				Application.Quit();
//				return;
//			}
//		}
		
		if(sapiInitialized)
		{
			// update the speech recognizer
			int rc = sensorData.sensorInterface.UpdateSpeechRecognition();
			
			if(rc >= 0)
			{
				// estimate the listening state
				if(sensorData.sensorInterface.IsSpeechStarted())
				{
					isListening = true;
				}
				else if(sensorData.sensorInterface.IsSpeechEnded())
				{
					isListening = false;
				}

				// check if a grammar phrase has been recognized
				if(sensorData.sensorInterface.IsPhraseRecognized())
				{
					isPhraseRecognized = true;
					phraseConfidence = sensorData.sensorInterface.GetPhraseConfidence();
					
					phraseTagRecognized = sensorData.sensorInterface.GetRecognizedPhraseTag();
					sensorData.sensorInterface.ClearRecognizedPhrase();
					
					//Debug.Log(phraseTagRecognized);
					if(debugText != null)
					{
						if(isPhraseRecognized)
						{
							debugText.text = string.Format("{0}  ({1:F1}%)", phraseTagRecognized, phraseConfidence * 100f);
						}
						else if(isListening)
						{
							debugText.text = "Listening...";
						}
					}

					// invoke SpeechPhraseRecognized() of the available speech listeners
					bool bClearPhrase = false;
					foreach(SpeechRecognitionInterface listener in speechRecognitionListeners)
					{
						if(listener.SpeechPhraseRecognized(phraseTagRecognized, phraseConfidence))
						{
							bClearPhrase = true;
						}
					}

					if(bClearPhrase)
					{
						ClearPhraseRecognized();
					}
				}

			}
		}
	}
	
//	void OnGUI()
//	{
//		if(sapiInitialized)
//		{
//			if(debugText != null)
//			{
//				if(isPhraseRecognized)
//				{
//					debugText.text = string.Format("{0}  ({1:F1}%)", phraseTagRecognized, phraseConfidence * 100f);
//				}
//				else if(isListening)
//				{
//					debugText.text = "Listening...";
//				}
//			}
//		}
//	}
	
	
}
