using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
	public enum GameAudioClip
	{
		MENU_BACKGROUND_MUSIC,
		GAME_START_SOUND
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

	private Dictionary<GameAudioClip, AudioClip> _gameAudioClips;

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

	private AudioSource _audioSource;

	private AudioSource AudioSource
	{
		get
		{
			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource> ();
			}

			return _audioSource;
		}
	}

	/// <summary>
	/// Plays the desired audio clip
	/// </summary>
	/// <param name="gameClip">Game clip to play.</param>
	public void PlayClip (GameAudioClip gameClip, bool loop =false)
	{
		AudioClip audioClip = null;

		if (GameAudioClips.TryGetValue (gameClip, out audioClip))
		{
			AudioSource.clip = audioClip;
			AudioSource.loop = loop;
			AudioSource.Play ();
		}
		else
		{
			Debug.LogWarningFormat ("AudioClipManager PlayClip: No clip found for: {0}", gameClip);
		}
	}
}
