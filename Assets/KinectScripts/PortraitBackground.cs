﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Sets the color background image in portrait mode. The aspect ratio of the game view should be set to 9:16 for Kinect-v2 or 3:4 for Kinect-v1.
/// </summary>
public class PortraitBackground : MonoBehaviour 
{
	[Tooltip("Whether to use the depth-image resolution in the calculation, instead of the color-image resolution.")]
	private bool useDepthImageResolution = false;

	private bool isInitialized = false;
	private Rect backgroundRect;
	private Rect inScreenRect;
	private Rect shaderUvRect;

	private static PortraitBackground instance = null;


	/// <summary>
	/// Gets the singleton PortraitBackground instance.
	/// </summary>
	/// <value>The PortraitBackground instance.</value>
	public static PortraitBackground Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// Determines whether the instance is initialized or not.
	/// </summary>
	/// <returns><c>true</c> if the instance is initialized; otherwise, <c>false</c>.</returns>
	public bool IsInitialized()
	{
		return isInitialized;
	}
	
	/// <summary>
	/// Gets the background rectangle in pixels. This rectangle can be provided as an argument to the GetJointPosColorOverlay()-KM function.
	/// </summary>
	/// <returns>The background rectangle, in pixels</returns>
	public Rect GetBackgroundRect()
	{
		return backgroundRect;
	}

	/// <summary>
	/// Gets the in-screen rectangle in pixels.
	/// </summary>
	/// <returns>The in-screen rectangle, in pixels.</returns>
	public Rect GetInScreenRect()
	{
		return inScreenRect;
	}

	/// <summary>
	/// Gets the shader uv rectangle. Can be used by custom shaders that need the portrait image uv-offsets and sizes.
	/// </summary>
	/// <returns>The shader uv rectangle.</returns>
	public Rect GetShaderUvRect()
	{
		return shaderUvRect;
	}


	////////////////////////////////////////////////////////////////////////


	void Awake()
	{
		instance = this;
	}

	void Start () 
	{
		KinectManager kinectManager = KinectManager.Instance;
		if(kinectManager && kinectManager.IsInitialized())
		{
			float fFactorDW = 0f;
			if(!useDepthImageResolution)
			{
				fFactorDW = (float)kinectManager.GetColorImageWidth() / (float)kinectManager.GetColorImageHeight() -
					(float)kinectManager.GetColorImageHeight() / (float)kinectManager.GetColorImageWidth();
			}
			else
			{
				fFactorDW = (float)kinectManager.GetDepthImageWidth() / (float)kinectManager.GetDepthImageHeight() -
					(float)kinectManager.GetDepthImageHeight() / (float)kinectManager.GetDepthImageWidth();
			}

			float fDeltaWidth = (float)Screen.height * fFactorDW;
			float dOffsetX = -fDeltaWidth / 2f;

			float fFactorSW = 0f;
			if(!useDepthImageResolution)
			{
				fFactorSW = (float)kinectManager.GetColorImageWidth() / (float)kinectManager.GetColorImageHeight();
			}
			else
			{
				fFactorSW = (float)kinectManager.GetDepthImageWidth() / (float)kinectManager.GetDepthImageHeight();
			}

			float fScreenWidth = (float)Screen.height * fFactorSW;
			float fAbsOffsetX = fDeltaWidth / 2f;

			GUITexture guiTexture = GetComponent<GUITexture>();
			if(guiTexture)
			{
				guiTexture.pixelInset = new Rect(dOffsetX, 0, fDeltaWidth, 0);
			}

			backgroundRect = new Rect(dOffsetX, 0, fScreenWidth, Screen.height);
			inScreenRect = new Rect(fAbsOffsetX, 0, fScreenWidth - fDeltaWidth, Screen.height);
			shaderUvRect = new Rect(fAbsOffsetX / fScreenWidth, 0, (fScreenWidth - fDeltaWidth) / fScreenWidth, 1);

			isInitialized = true;
		}
	}
}
