using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreUIView : UIView
{
	[Header("Highscore View")]
	[SerializeField]
	private Button _continueButton;

	private void Awake ()
	{
		_continueButton.onClick.AddListener (() => {
			GameManager.Instance.GoToState (GameState.START_MENU);
		});
	}
}
