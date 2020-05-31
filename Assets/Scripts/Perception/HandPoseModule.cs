/*
This module simply starts/stops the hand pose python client.

Writes:		
    user:hands:left:argmax
    user:hands:left:label
    user:hands:right:argmax
    user:hands:right:label

Reads: 
    From kinect directly (nothing from blackboard)
*/

using UnityEngine;

public class HandPoseModule : ModuleBase
{
	private ExternalProcess handPoseRecognizer;
	private bool endedExternally = false;
	private int lastOutputLogLength = 0;
	private int lastErrorLogLength = 0;

	protected override void Start()
	{
		base.Start();

		string python = PlayerPrefs.GetString("pythonPath", "python.exe");
		if (string.IsNullOrEmpty(python)) {
			Debug.Log("Python path is empty; skipping " + gameObject.name);
			endedExternally = true;
			return;		// leaves handPoseRecognizer == null, so let's be prepared for that!
		}

		handPoseRecognizer = new ExternalProcess(
			pathToExecutable: python,
			arguments: "-u -m External.Perception.HandPose.depth_client",
			redirectStandardOutput: true
		);

		handPoseRecognizer.Hide = true;

		handPoseRecognizer.Start();

		if (handPoseRecognizer.HasStarted)
		{
			Debug.Log("Started hand pose client");
		}
		else
		{
			Debug.LogWarning("Error starting hand pose client: " + handPoseRecognizer.ErrorLog);
		}
	}

	protected void Update()
	{
		if (handPoseRecognizer == null) return;
		if (!endedExternally && handPoseRecognizer.HasExited)
		{
			Debug.LogError("Hand pose client exited unexpectedly");
			endedExternally = true;
		}
		else if (handPoseRecognizer.OutputLog != null)
		{
			int currentOutputLogLength = handPoseRecognizer.OutputLog.Length;
			if (currentOutputLogLength > lastOutputLogLength)
			{
				Debug.Log("Hand pose stdout: " + handPoseRecognizer.OutputLog.Substring(lastOutputLogLength, currentOutputLogLength - lastOutputLogLength));
				lastOutputLogLength = currentOutputLogLength;
			}

			int currentErrorLogLength = handPoseRecognizer.ErrorLog.Length;
			if (currentErrorLogLength > lastErrorLogLength)
			{
				Debug.Log("Hand pose stderr: " + handPoseRecognizer.ErrorLog.Substring(lastErrorLogLength, currentErrorLogLength - lastErrorLogLength));
				lastErrorLogLength = currentErrorLogLength;
			}
		}
	}

	private void OnDestroy()
	{
		if (handPoseRecognizer != null && handPoseRecognizer.HasStarted && !handPoseRecognizer.HasExited)
		{
			handPoseRecognizer.Close();
			Debug.Log("Hand pose client closed");
		}
	}
}
