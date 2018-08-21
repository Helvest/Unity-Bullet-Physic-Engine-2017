using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenu : MonoBehaviour
{
	public ScenesList scenes;

	public GUILayoutOption w = GUILayout.MinWidth(200);
	public GUILayoutOption h = GUILayout.MinHeight(64);

	public void Start()
	{
		scenes = Resources.Load<ScenesList>("ScenesList");
	}

	public void OnGUI()
	{
		// Create a button for each scene.
		//GUI.BeginGroup(new Rect(Screen.width/2 - 150, Screen.height/2, 300, 500));
		GUILayout.BeginHorizontal(GUILayout.Width(1000));

		int i = 0;
		int maxPerColumn = 6;
		while (i < scenes.Scenes.Count)
		{
			GUILayout.BeginVertical("box", w);

			for (int j = 0; j < maxPerColumn && i < scenes.Scenes.Count; j++, i++)
			{
				if (GUILayout.Button(scenes.Scenes[i].Name, w, h))
				{
					Debug.Log("Loading level: " + scenes.Scenes[i].Path);
					SceneManager.LoadScene(i);
				}
			}

			GUILayout.EndVertical();
		}

		GUILayout.EndHorizontal();
	}
}
