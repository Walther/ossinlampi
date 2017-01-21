using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using Pitch;

[RequireComponent(typeof(AudioSource))]
public class VoiceController : Singleton<VoiceController>
{
	public class VoiceEvent
	{
		public float volume;
		public float frequency;
	}

	#region public fields

	// Written in part by Benjamin Outram
	public bool enableDebug;

	// Option to toggle the microphone listenter on startup or not
	public bool startMicOnStartup = true;

	// Allows start and stop of listener at run time within the unity editor
	public bool stopMicrophoneListener = false;
	public bool startMicrophoneListener = false;

	private bool microphoneListenerOn = false;

	// Public to allow temporary listening over the speakers if you want of the mic output
	// but internally it toggles the output sound to the speakers of the audiosource depending
	// on if the microphone listener is on or off
	public bool disableOutputSound = false;

	// Make an audio mixer from the "create" menu, then drag it into the public field on this script.
	// Double click the audio mixer and next to the "groups" section, click the "+" icon to add a 
	// child to the master group, rename it to "microphone".  Then in the audio source, in the "output" option, 
	// select this child of the master you have just created.
	// go back to the audiomixer inspector window, and click the "microphone" you just created, then in the 
	// inspector window, right click "Volume" and select "Expose Volume (of Microphone)" to script,
	// then back in the audiomixer window, in the corner click "Exposed Parameters", click on the "MyExposedParameter"
	// and rename it to "Volume"
	public AudioMixer masterMixer;

	[Header("Microphone")]
	[Tooltip("Microphone buffer length in seconds")]
	public int bufferLengthSec = 1;
	[Tooltip("Microphone sampling frequency")]
	public int samplingFrequency = 44100;

    [Header("Pitch detection")]
    [Tooltip("Pitch detection window size")]
    public int outputBufferSize = 512;

	[Header("FFT")]
	[Tooltip("FFT spectrum window size")]
	public int spectrumWindowSize = 512;



	[Tooltip("FFT window type")]
	public FFTWindow fftWindowType = FFTWindow.Hamming;
	[Tooltip("FFT spectrum scale factor for debug")]
	public float debugScale = 2.0f;

	[Header("Game Play")]
	[Tooltip("Shooting threshold calculated from FFT spectrum average")]
	public float shootingThreshold = 3.0f;

	public IObservable<VoiceEvent> AboveThresholdStream
	{
		get
		{
			return AboveThresholdSubject.AsObservable ();
		}
	}

	#endregion

	#region private fields

	private float[] _spectrum = null;

    private float[] _output = null;

	private AudioSource _src = null;
	private float _timeSinceRestart = 0;
	private Subject<VoiceEvent> _aboveThresholdSubject = null;

    private PitchTracker _pitchTracker;

    private float _volume = 0;

	private AudioSource Source
	{
		get
		{
			if (_src == null)
			{
				_src = GetComponent<AudioSource> ();
			}

			return _src;
		}
	}

	private AudioClip Clip
	{
		get
		{
			return _src.clip;
		}
	}

	private float[] Spectrum
	{
		get
		{
			if (_spectrum == null)
			{
				_spectrum = new float[spectrumWindowSize];
			}

			return _spectrum;
		}
	}

    private float[] Output
    {
        get
        {
            if (_output == null)
            {
                _output = new float[outputBufferSize];
            }

            return _output;
        }
    }

	private Subject<VoiceEvent> AboveThresholdSubject
	{
		get
		{
			if (_aboveThresholdSubject == null)
			{
				_aboveThresholdSubject = new Subject<VoiceEvent> ();
			}

			return _aboveThresholdSubject;
		}
	}

	#endregion

	private void Start ()
	{        
		// Start the microphone listener
		if (startMicOnStartup)
		{
			RestartMicrophoneListener ();
			StartMicrophoneListener ();
		}

        _pitchTracker = new PitchTracker();
        _pitchTracker.SampleRate = 44100;
        _pitchTracker.DetectLevelThreshold = 0.1f;
        _pitchTracker.PitchDetected += (sender, pitchRecord) => 
            {
                if (pitchRecord.Pitch > 0)
                {
                    AboveThresholdSubject.OnNext (new VoiceEvent {
                        volume = pitchRecord.Volume,
                        frequency = pitchRecord.Pitch
                    });
                }
            };
	}
		
	private void Update ()
	{
		// Can use these variables that appear in the inspector,
		// or can call the public functions directly from other scripts
		if (stopMicrophoneListener)
		{
			StopMicrophoneListener ();
		}

		if (startMicrophoneListener)
		{
			StartMicrophoneListener ();
		}

		// Reset paramters to false because only want to execute once
		stopMicrophoneListener = false;
		startMicrophoneListener = false;

		// Must run in update otherwise it doesnt seem to work
		MicrophoneIntoAudioSource (microphoneListenerOn);

		// Can choose to unmute sound from inspector if desired
		DisableSound (!disableOutputSound);

//		Source.GetSpectrumData (Spectrum, 0, fftWindowType);
        Source.GetOutputData(Output, 0);

        _volume = 0;
        for (int i = 0; i < Output.Length; ++i)
        {
            _volume += Mathf.Abs(Output[i]);
        }
        _volume /= Output.Length;

        _pitchTracker.ProcessBuffer(Output, _volume);


//		float volume = Spectrum.Average ();
//
//		if (volume > shootingThreshold)
//		{
//			// Calculate frequency and send an event/publish to subject
//			AboveThresholdSubject.OnNext (new VoiceEvent {
//				volume = volume,
//				frequency = GetSpectrumMaxFrequency ()
//			});
//		}
//
//		// Log to debug if debug is enabled
//		if (enableDebug)
//		{
//			for (int i = 1; i < spectrumWindowSize-1; ++i)
//			{
//				Debug.DrawLine (
//					new Vector3(Mathf.Log(i-1), (debugScale * Spectrum[i]), 0),
//					new Vector3(Mathf.Log(i), (debugScale * Spectrum[i + 1]), 0),
//					Color.red
//				);
//			}
//
//			Debug.LogFormat ("Volume: {0} - Above threshold: {1}", volume, volume > shootingThreshold);
//			Debug.LogFormat ("Frequency: {0} Hz", GetSpectrumMaxFrequency ());
//		}
	}

	public float GetSpectrumMaxFrequency ()
	{
		float binWidth = (float)samplingFrequency/(float)spectrumWindowSize;
		float maxBinValue = 0.0f;
		int maxBinIdx = 0;

		for (int i = 0; i < spectrumWindowSize; ++i)
		{
			if (Spectrum[i] > maxBinValue)
			{
				maxBinIdx = i;
				maxBinValue = Spectrum[i];
			}
		}

		// Nobody knows why we need to scale by 0.5f - but it sure works great
		float frequency = (binWidth*maxBinIdx + (0.5f*binWidth)) * 0.5f;
		return frequency;
	}

	/// <summary>
	/// Stops everything and returns audioclip to null
	/// </summary>
	public void StopMicrophoneListener ()
	{
		//stop the microphone listener
		microphoneListenerOn = false;
		//reenable the master sound in mixer
		disableOutputSound = false;
		//remove mic from audiosource clip
		Source.Stop ();
		Source.clip = null;

		Microphone.End (null);
	}

	/// <summary>
	/// Starts the microphone listener.
	/// </summary>
	public void StartMicrophoneListener ()
	{
		// Start the microphone listener
		microphoneListenerOn = true;
		// Disable sound output (dont want to hear mic input on the output!)
		disableOutputSound = true;
		// Reset the audiosource
		RestartMicrophoneListener ();
	}

	/// <summary>
	/// Controls whether the volume is on or off, use "off" for mic input (dont want to hear your own voice input!) 
	/// and "on" for music input
	/// </summary>
	/// <param name="SoundOn">If set to <c>true</c> sound on.</param>
	public void DisableSound (bool SoundOn)
	{
		float volume = SoundOn ? 0.0f : -80.0f;
		masterMixer.SetFloat ("MasterVolume", volume);
	}

	/// <summary>
	/// Restart microphone removes the clip from the audiosource
	/// </summary>
	public void RestartMicrophoneListener ()
	{
		// Remove any soundfile in the audiosource
		Source.clip = null;
		_timeSinceRestart = Time.time;
	}

	/// <summary>
	/// Places the mic into the AudioSource
	/// </summary>
	private void MicrophoneIntoAudioSource (bool MicrophoneListenerOn)
	{
		if (MicrophoneListenerOn)
		{
			// Pause a little before setting clip to avoid lag and bugginess
			if (Time.time - _timeSinceRestart > 0.5f && !Microphone.IsRecording (null))
			{
				Source.clip = Microphone.Start (null, true, bufferLengthSec, samplingFrequency);

				// Wait until microphone position is found (?)
				while (!(Microphone.GetPosition (null) > 0));

				// Play the audio source
				Source.Play ();
			}
		}
	}
}