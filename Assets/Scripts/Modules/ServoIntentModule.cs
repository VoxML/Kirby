﻿using UnityEngine;

public class ServoIntentModule : ModuleBase
{
    // for tuning
    public int servoLeftTimeToStart, servoLeftTimeToStop;
    public int servoRightTimeToStart, servoRightTimeToStop;
    public int servoBackTimeToStart, servoBackTimeToStop;
    public bool logChanges;

    private ServoLeftStateMachine servoLeftStateMachine;
	private ServoRightStateMachine servoRightStateMachine;
	private ServoBackStateMachine servoBackStateMachine;

	protected override void Start()
    {
		base.Start();

		servoLeftStateMachine = new ServoLeftStateMachine(servoLeftTimeToStart, servoLeftTimeToStop);
		servoRightStateMachine = new ServoRightStateMachine(servoRightTimeToStart, servoRightTimeToStop);
		servoBackStateMachine = new ServoBackStateMachine(servoBackTimeToStart, servoBackTimeToStop);
	}

	private void Update()
	{
		if (servoRightStateMachine.Evaluate())
		{
			switch (servoRightStateMachine.CurrentState)
			{
				case ServoRightState.ServoRightStart:
					DataStore.SetValue("user:intent:isServoRight", DataStore.BoolValue.True, this, "servo right with left hand");
					break;
				case ServoRightState.ServoRightStop:
					DataStore.SetValue("user:intent:isServoRight", DataStore.BoolValue.False, this, "stopped servo right with left hand");
					break;
			}

            if (logChanges)
            {
                Debug.Log(string.Format("Servo right state change: {0}", DataStore.GetBoolValue("user:intent:isServoRight")));
            }
        }

		if (servoLeftStateMachine.Evaluate())
		{
			switch (servoLeftStateMachine.CurrentState)
			{
				case ServoLeftState.ServoLeftStart:
					DataStore.SetValue("user:intent:isServoLeft", DataStore.BoolValue.True, this, "servo left with right hand");
					break;
				case ServoLeftState.ServoLeftStop:
					DataStore.SetValue("user:intent:isServoLeft", DataStore.BoolValue.False, this, "stopped servo left with right hand");
					break;
			}

            if (logChanges)
            {
                Debug.Log(string.Format("Servo left state change: {0}", DataStore.GetBoolValue("user:intent:isServoLeft")));
            }
        }

		if (servoBackStateMachine.Evaluate())
		{
			switch (servoBackStateMachine.CurrentState)
			{
				case ServoBackState.ServoBackStart:
					DataStore.SetValue("user:intent:isServoBack", DataStore.BoolValue.True, this, "servo back with either hand");
					break;
				case ServoBackState.ServoBackStop:
					DataStore.SetValue("user:intent:isServoBack", DataStore.BoolValue.False, this, "stopped servo back");
					break;
			}

            if (logChanges)
            {
                Debug.Log(string.Format("Servo back state change: {0}", DataStore.GetBoolValue("user:intent:isServoBack")));
            }
        }
	}
}
