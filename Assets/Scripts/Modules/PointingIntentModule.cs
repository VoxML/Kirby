using UnityEngine;

public class PointingIntentModule: ModuleBase
{
    public GameObject PointingMarker;
    public float StableLocationConstant = .05f;

	private PointingStateMachine pointingStateMachine;

	protected override void Start()
	{
		base.Start();
		pointingStateMachine = new PointingStateMachine();
        pointingStateMachine.StableLocationConstant = StableLocationConstant;
        pointingStateMachine.MarkerObj = PointingMarker;

    }

	private void Update()
	{
        if (pointingStateMachine.Evaluate())
		{
			switch (pointingStateMachine.CurrentState)
			{
				case PointingState.Pointed:
				case PointingState.PointingStop:
                    DataStore.SetValue("user:lastPointedAt:name", new DataStore.StringValue(pointingStateMachine.LastPointedAtObject?.name ?? string.Empty), this, string.Empty);
                    DataStore.SetValue("user:lastPointedAt:position", new DataStore.Vector3Value(pointingStateMachine.LastPointedAtLocation), this, string.Empty);
                    break;
				default:
					break;
			}
		}
	}

}
