using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
	string scene;
	public void LoadScene(string _scene)
	{
		scene = _scene;
		Scene thisScene = SceneManager.GetActiveScene();
		StartCoroutine(LoadNewScene());
	}

	public void QuitApp()
	{
		Application.Quit();
	}

	// The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
	System.Collections.IEnumerator LoadNewScene()
	{
		// Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);

		// While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
		while (!async.isDone)
		{
			yield return null;
		}
	}
}
