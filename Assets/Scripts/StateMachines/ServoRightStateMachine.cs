﻿public enum ServoRightState
{
	ServoRightStop,
	ServoRightStart
}

public class ServoRightStateMachine : RuleStateMachine<ServoRightState>
{
	public ServoRightStateMachine(int timeToStart, int timeToStop)
	{
		SetTransitionRule(ServoRightState.ServoRightStop, ServoRightState.ServoRightStart, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (userIsEngaged)
			{
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				string leftArmServo = DataStore.GetStringValue("user:armMotion:left");
				return (leftHandGesture == "open right" || leftHandGesture == "closed right") && leftArmServo == "servo";
			}
			return false;
		}, timeToStart));

		SetTransitionRule(ServoRightState.ServoRightStart, ServoRightState.ServoRightStop, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (!userIsEngaged)
				return true;
			else
			{
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				string leftArmServo = DataStore.GetStringValue("user:armMotion:left");
				return (leftHandGesture != "open right" && leftHandGesture != "closed right") || leftArmServo != "servo";
			}
		}, timeToStop));
	}
}