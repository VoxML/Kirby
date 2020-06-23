public enum ServoBackState
{
	ServoBackStop,
	ServoBackStart
}

public class ServoBackStateMachine : RuleStateMachine<ServoBackState>
{
	public ServoBackStateMachine(int timeToStart, int timeToStop)
	{
		SetTransitionRule(ServoBackState.ServoBackStop, ServoBackState.ServoBackStart, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (userIsEngaged)
			{
				string rightHandGesture = DataStore.GetStringValue("user:hands:right");
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				return rightHandGesture == "beckon" || leftHandGesture == "beckon";
			}
			return false;
		}, timeToStart));

		SetTransitionRule(ServoBackState.ServoBackStart, ServoBackState.ServoBackStop, new TimedRule(() =>
		{
			bool userIsEngaged = DataStore.GetBoolValue("user:isEngaged");

			if (!userIsEngaged)
				return true;
			else
			{
				string rightHandGesture = DataStore.GetStringValue("user:hands:right");
				string leftHandGesture = DataStore.GetStringValue("user:hands:left");
				return rightHandGesture != "beckon" && leftHandGesture != "beckon";
			}
		}, timeToStop));
	}
}