﻿public enum PushRightState
{
	PushRightStop,
	PushRightStart
}

public class PushRightStateMachine : RuleStateMachine<PushRightState>
{
	public PushRightStateMachine()
	{
		SetTransitionRule(PushRightState.PushRightStop, PushRightState.PushRightStart, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (userIsEngaged)
			{
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				string leftArmMotion = DataStore.GetStringValue("user:arms:left");
				return (leftHandGesture == "open right" || leftHandGesture == "closed right" 
					|| leftHandGesture == "open back" || leftHandGesture == "closed back") 
					&& leftArmMotion == "move right";
			}
			return false;
		}, 200));

		SetTransitionRule(PushRightState.PushRightStart, PushRightState.PushRightStop, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (!userIsEngaged)
				return true;
			else
			{
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				string leftArmMotion = DataStore.GetStringValue("user:arms:left");
				return (leftHandGesture != "open right" && leftHandGesture != "closed right" 
					&& leftHandGesture != "open back" && leftHandGesture != "closed back") 
					|| leftArmMotion != "move right";
			}
		}, 100));
	}
}
