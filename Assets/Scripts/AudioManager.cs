using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
	public enum GameAudioClip
	{
		MENU_BACKGROUND_MUSIC,
		GAME_START_SOUND,
		PLAYER_FIRE
	}

	[System.Serializable]
	protected class AudioManagerClip
	{
		public GameAudioClip gameAudioClip;
		public AudioClip audioClip;
	}
		
	// Dictionaries don't serialize well in Unity inspector - use array and postprocess
	// to dictionary
	[SerializeField]
	private AudioManagerClip[] _gameAudioClipArray;
	[SerializeField]
	private GameObject _audioSourcePrefab;
	[SerializeField]
	private AudioSource _backgroundAudioSource;

	private Dictionary<GameAudioClip, AudioClip> _gameAudioClips;
	private List<GameObject> _audioSourcePool;

	private Dictionary<GameAudioClip, AudioClip> GameAudioClips
	{
		get
		{
			if (_gameAudioClips == null)
			{
				// Populate the dictionary
				_gameAudioClips = new Dictionary<GameAudioClip, AudioClip> ();

				int numAudioClips = _gameAudioClipArray.Length;

				for (int i = 0; i < numAudioClips; ++i)
				{
					if (_gameAudioClipArray[i].audioClip != null)
					{
						_gameAudioClips.Add (_gameAudioClipArray[i].gameAudioClip, _gameAudioClipArray[i].audioClip);
					}
					else
					{
						Debug.LogWarningFormat ("AudioManager GameAudioClips: Null clip for GameAudioClip {0}", _gameAudioClipArray[i].gameAudioClip);
					}
				}
			}

			return _gameAudioClips;
		}
	}

	private List<GameObject> AudioSourcePool
	{
		get
		{
			if (_audioSourcePool == null)
			{
				_audioSourcePool = new List<GameObject> ();
			}

			return _audioSourcePool;
		}
	}

	private AudioSource GetPooledAudioSource ()
	{
		GameObject freeAudioSource = AudioSourcePool.Find (src => !src.GetComponent<AudioSource> ().isPlaying);

		if (freeAudioSource == null)
		{
			freeAudioSource = Instantiate (_audioSourcePrefab, transform) as GameObject;
			AudioSource freeSource = freeAudioSource.GetComponent<AudioSource> ();
			AudioSourcePool.Add (freeAudioSource);
		}

		return freeAudioSource.GetComponent<AudioSource> ();
	}

	/// <summary>
	/// Plays the desired audio clip
	/// </summary>
	/// <param name="gameClip">Game clip to play.</param>
	public void PlayClip (GameAudioClip gameClip)
	{
		AudioClip audioClip = null;

		if (GameAudioClips.TryGetValue (gameClip, out audioClip))
		{
			PlayClip (audioClip);
		}
		else
		{
			Debug.LogWarningFormat ("AudioClipManager PlayClip: No clip found for: {0}", gameClip);
		}
	}

	/// <summary>
	/// Plays the audio clip
	/// </summary>
	/// <param name="audioClip">Audio clip</param>
	public void PlayClip (AudioClip audioClip)
	{
		if (audioClip != null)
		{
			AudioSource source = GetPooledAudioSource ();
			source.clip = audioClip;
			source.Play ();
		}
	}

	/// <summary>
	/// Plays the desired audio clip
	/// </summary>
	/// <param name="gameClip">Game clip to play.</param>
	public void PlayBackgroundClip (GameAudioClip gameClip, bool loop =false)
	{
		AudioClip audioClip = null;

		if (GameAudioClips.TryGetValue (gameClip, out audioClip))
		{
			PlayBackgroundClip (audioClip, loop);
		}
		else
		{
			Debug.LogWarningFormat ("AudioClipManager PlayBackgroundClip: No clip found for: {0}", gameClip);
		}
	}

	/// <summary>
	/// Plays the audio clip in the background. Stops previous clip if there is any.
	/// </summary>
	/// <param name="audioClip">Audio clip.</param>
	/// <param name="loop">If set to <c>true</c> loop.</param>
	public void PlayBackgroundClip (AudioClip audioClip, bool loop)
	{
		if (audioClip != null)
		{
			_backgroundAudioSource.clip = audioClip;
			_backgroundAudioSource.loop = loop;
			_backgroundAudioSource.Play ();
		}
	}

	/// <summary>
	/// Stops the background clip.
	/// </summary>
	public void StopBackgroundClip ()
	{
		_backgroundAudioSource.Stop ();
	}
}
