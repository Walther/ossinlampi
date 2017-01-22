using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIView : UIView
{
	[Header("Game UI")]
	[SerializeField]
	private Text _scoreText;
	[SerializeField]
	private Text _hpText;

	public void SetScore (int newScore)
	{
		_scoreText.text = string.Format ("Score: {0} PTS", newScore);
	}

	public void SetHp (float hp)
	{
		_hpText.text = string.Format ("HP: {0}", Mathf.Clamp ((int)hp, 0, int.MaxValue));
	}
}
