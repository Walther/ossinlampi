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

	[Header("General")]
	[SerializeField]
	private PlayerController	_player				= null;
	[SerializeField]
	private ParticleSystem		_fireworks			= null;

	private GameState 			_currentState 		= GameState.NONE;
	private int					_currentScore		= 0;

	public GameState CurrentState
	{
		get
		{
			return _currentState;
		}
	}

	public PlayerController CurrentPlayer
	{
		get
		{
			return _player;
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

			// Play background music
			AudioManager.Instance.PlayBackgroundClip (AudioManager.GameAudioClip.MENU_BACKGROUND_MUSIC, true);
		}
		// Start menu play was pressed - transition to playing
		else if (_currentState == GameState.START_MENU &&
			newState == GameState.PLAYING)
		{
			// Reset
			_currentScore = 0;
			ResetGameUIView ();

			// Transition start menu out and game UI in
			_startMenuUIView
				.TransitionOut ()
				.Concat (_gameUIView.TransitionIn ())
				.Subscribe ();

			// Stop background and play game start
			AudioManager.Instance.StopBackgroundClip ();
			AudioManager.Instance.PlayClip (AudioManager.GameAudioClip.GAME_START_SOUND);

			// Start spawning enemies
			EnemyManager.Instance.StartSpawningEnemies ();
		}
		// If the player died i.e. game over
		else if (_currentState == GameState.PLAYING &&
			newState == GameState.GAME_OVER)
		{
			_gameUIView
				.TransitionOut ()
				.Concat (_highscoreUIView.TransitionIn ())
				.Subscribe ();
			
			_highscoreUIView.SetScore (_currentScore);
			_fireworks.gameObject.SetActive (true);
			_fireworks.Play (true);
		}
		else if (_currentState == GameState.GAME_OVER &&
			newState == GameState.START_MENU)
		{
			_fireworks.gameObject.SetActive (false);
			_fireworks.Clear ();

			// Clear any ongoing waves
			EnemyManager.Instance.ClearEnemies ();

			// Reset
			CurrentPlayer.Respawn ();
			_currentScore = 0;
			ResetGameUIView ();

			// Transition start menu out and game UI in
			_highscoreUIView
				.TransitionOut ()
				.Concat (_startMenuUIView.TransitionIn ())
				.Subscribe ();
		
			// Play background music
			AudioManager.Instance.PlayBackgroundClip (AudioManager.GameAudioClip.MENU_BACKGROUND_MUSIC, true);
		}

		_currentState = newState;
	}

	public void DamagePlayer (float damage)
	{
		CurrentPlayer.TakeDamage (damage);
		_gameUIView.SetHp (CurrentPlayer.CurrentHp);
	}

	public void AddScore (int score)
	{
		_currentScore += score;
		_gameUIView.SetScore (_currentScore);
	}

	private void ResetGameUIView ()
	{
		_gameUIView.SetHp (CurrentPlayer.CurrentHp);
		_gameUIView.SetScore (0);
	}
}
