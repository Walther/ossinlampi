using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUIView : UIView
{
	[Header("Start Menu View")]
	[SerializeField]
	private Button _playButton;

	private void Awake ()
	{
		_playButton.onClick.AddListener (() => {
			GameManager.Instance.GoToState (GameState.PLAYING);
		});
	}
}
