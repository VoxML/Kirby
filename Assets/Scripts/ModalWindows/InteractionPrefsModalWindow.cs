using UnityEngine;
using System.Collections.Generic;

using VoxSimPlatform.Agent.SyntheticVision;
using VoxSimPlatform.UI.ModalWindow;

public class InteractionPrefsModalWindow : ModalWindow {
	public int fontSize = 12;

	GUIStyle buttonStyle;

	float fontSizeModifier;

	public float FontSizeModifier {
		get { return fontSizeModifier; }
		set { fontSizeModifier = value; }
	}

	//string[] verbosityListItems = {"Everything", "Disambiguation only", "None"};
	//string[] disambiguationListItems = {"Elimination", "Deictic/Gestural"};
	//string[] deixisListItems = {"Screen", "Table"};

	List<string> programs = new List<string>();

	//public List<string> Programs {
	//	get { return programs; }
	//	set {
	//		programs = value;
	//		verbosityListItems = programs.ToArray();
	//	}
	//}

	//public enum VerbosityLevel {
	//	Everything,
	//	Disambiguation,
	//	None
	//};

	//public enum DisambiguationStrategy {
	//	Elimination,
	//	DeicticGestural
	//};

	//public enum DeixisMethod {
	//	Screen,
	//	Table
	//};

	public string userName = "";
	//public VerbosityLevel verbosityLevel = VerbosityLevel.Disambiguation;
	//public DisambiguationStrategy disambiguationStrategy = DisambiguationStrategy.DeicticGestural;
	//public DeixisMethod deixisMethod = DeixisMethod.Screen;

	//public bool useTeachingAgent = false;

	bool showSyntheticVision = true;

    public bool ShowInsetCamera
    {
        get { return showSyntheticVision; }
        set
        {
            showSyntheticVision = value;
            synVision.ShowFoV = showSyntheticVision;
        }
    }

    bool showOnboardCameraView = true;

    public bool ShowOnboardCameraView
    {
        get { return showOnboardCameraView; }
        set
        {
            showOnboardCameraView = value;
            cameraManager.showPiCamView = showOnboardCameraView;
        }
    }

    //public bool showVisualMemory = false;
    //public bool visualizeDialogueState = false;
    public bool connectionLostNotification = true;

	public bool linguisticReference = true;
	public bool gesturalReference = true;

	string actionButtonText;

    SyntheticVision synVision;
    CameraManager cameraManager;

	// Use this for initialization
	void Start() {
		base.Start();

		actionButtonText = "Interaction Prefs";
		windowTitle = "Kirby Interaction Prefs";
		persistent = true;

		fontSizeModifier = (int) (fontSize / defaultFontSize);

		windowRect = new Rect(Screen.width - 230, 110 + (int) (20 * fontSizeModifier), 215, 200);

        synVision = GameObject.Find("RoboCamera").GetComponent<SyntheticVision>();
        synVision.ShowFoV = showSyntheticVision;

        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        cameraManager.showPiCamView = showOnboardCameraView;
    }

	// Update is called once per frame
	void Update() {
	}

	protected override void OnGUI() {
		buttonStyle = new GUIStyle("Button");
		buttonStyle.fontSize = fontSize;

		if (GUI.Button(new Rect(
				Screen.width - (10 + (int) (110 * fontSizeModifier / 3)) + 36 * fontSizeModifier -
				(GUI.skin.label.CalcSize(new GUIContent(actionButtonText)).x + 10),
				100, GUI.skin.label.CalcSize(new GUIContent(actionButtonText)).x + 10, 25 * fontSizeModifier),
			actionButtonText, buttonStyle)) {
			render = true;
		}

		base.OnGUI();
	}

	public override void DoModalWindow(int windowID) {
		base.DoModalWindow(windowID);

		//makes GUI window scrollable
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("User Name:");
		userName = GUILayout.TextField(userName,
			GUILayout.Width(this.windowRect.width - GUI.skin.label.CalcSize(new GUIContent("User Name:")).x - 50),
			GUILayout.ExpandWidth(false));
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label("Agent Perception:");
		GUILayout.BeginVertical(GUI.skin.box);
		ShowInsetCamera =
			GUILayout.Toggle(ShowInsetCamera, "Show Inset Camera", GUILayout.ExpandWidth(true));
        ShowOnboardCameraView =
            GUILayout.Toggle(ShowOnboardCameraView, "Show Onboard Camera", GUILayout.ExpandWidth(true));
        GUILayout.EndVertical();
		GUILayout.EndVertical();

		GUILayout.BeginHorizontal(GUI.skin.box);
		connectionLostNotification = GUILayout.Toggle(connectionLostNotification, "Connection Lost Notification",
			GUILayout.ExpandWidth(true));
		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();
	}
}