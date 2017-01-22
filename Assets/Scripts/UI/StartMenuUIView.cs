using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUIView : UIView
{
	[Header("Start Menu View")]
	[SerializeField]
	private Button _playButton;
	[SerializeField]
	private Button _toggleModeButton;
	[SerializeField]
	private Text _toggleModeButtonText;

	private bool _retroOn;

	private void Awake ()
	{
		_retroOn = Camera.main.targetTexture != null;
		_toggleModeButtonText.text = _retroOn ? "HIGH-DEF" : "RETRO";

		_playButton.onClick.AddListener (() => {
			GameManager.Instance.GoToState (GameState.PLAYING);
		});

		_toggleModeButton.onClick.AddListener (() => {
			_retroOn = !_retroOn;
			_toggleModeButtonText.text = _retroOn ? "HIGH-DEF" : "RETRO";
			GameManager.Instance.SetRetroMode (_retroOn);
		});
	}
}
