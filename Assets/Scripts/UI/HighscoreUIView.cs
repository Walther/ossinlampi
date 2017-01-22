using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreUIView : UIView
{
	[Header("Highscore View")]
	[SerializeField]
	private Button _continueButton;
	[SerializeField]
	private Text _scoreText;
	[SerializeField]
	private Text _newHighscoreText;

	private void Awake ()
	{
		_continueButton.onClick.AddListener (() => {
			GameManager.Instance.GoToState (GameState.START_MENU);
		});
	}

	public void SetScore (int score, int previousBest)
	{
		_scoreText.text = string.Format ("{0} PTS", score);
		_newHighscoreText.gameObject.SetActive (score > previousBest);
	}
}
