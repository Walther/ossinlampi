using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIView : UIView
{
	[Header("Game UI")]
	[SerializeField]
	private Text _scoreText;

	public void SetScore (int newScore)
	{
		_scoreText.text = string.Format ("Score: {0} PTS", newScore);
	}
}
