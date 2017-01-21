using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum GameState
{
	NONE		= 0,
	START_MENU 	= 1,
	PLAYING 	= 2,
	GAME_OVER 	= 3
}

public class GameManager : Singleton<GameManager>
{
	[Header("UI Views")]
	[SerializeField]
	private StartMenuUIView 	_startMenuUIView 	= null;
	[SerializeField]
	private GameUIView 			_gameUIView			= null;
	[SerializeField]
	private HighscoreUIView 	_highscoreUIView	= null;

	private GameState 			_currentState 		= GameState.NONE;

	public GameState CurrentState
	{
		get
		{
			return _currentState;
		}
	}

	private void Awake ()
	{
		GoToState (GameState.START_MENU);
	}

	public void GoToState (GameState newState)
	{
		// Initial start of the game
		if (_currentState == GameState.NONE &&
			newState == GameState.START_MENU)
		{
			_startMenuUIView
				.TransitionIn ()
				.Subscribe ();
		}
		// Start menu play was pressed - transition to playing
		else if (_currentState == GameState.START_MENU &&
			newState == GameState.PLAYING)
		{
			_startMenuUIView
				.TransitionOut ()
				.Concat (_gameUIView.TransitionIn ())
				.Subscribe ();
		}

		_currentState = newState;
	}
}
