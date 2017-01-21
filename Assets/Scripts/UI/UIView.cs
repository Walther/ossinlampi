using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIView : MonoBehaviour
{
	[Header("UIView")]

	[SerializeField]
	[Tooltip("How long are the transitions in seconds")]
	[Range(0.1f, float.MaxValue)]
	protected float _transitionDuration;

	protected CanvasGroup 	_uiCanvasGroup;
	protected Tweener		_currentTween;

	protected CanvasGroup UICanvasGroup
	{
		get
		{
			if (_uiCanvasGroup == null)
			{
				_uiCanvasGroup = GetComponent<CanvasGroup> ();
			}

			return _uiCanvasGroup;
		}
	}

	/// <summary>
	/// Shows the UI interface by fading in and enables interaction
	/// </summary>
	/// <returns>An observable sequence that completes when transition is complete.</returns>
	virtual public IObservable<Unit> TransitionIn ()
	{
		return Observable.Create<Unit> (observer => {
			if (_currentTween != null)
			{
				_currentTween.Kill ();
				_currentTween = null;
			}

			_currentTween = UICanvasGroup.DOFade (1.0f, _transitionDuration)
				.OnStart (() => {
					UICanvasGroup.alpha = 0.0f;
					gameObject.SetActive (true);
				})
				.OnComplete (() => {
					UICanvasGroup.interactable = true;
					observer.OnNext (Unit.Default);
					observer.OnCompleted ();
				});

			return Disposable.Create (() => {
				if (_currentTween != null)
				{
					_currentTween.Kill ();
					_currentTween = null;
				}
			});
		});
	}

	/// <summary>
	/// Hides the UI interface by fading out and disables interaction
	/// </summary>
	/// <returns>An observable sequence that completes when transition is complete.</returns>
	virtual public IObservable<Unit> TransitionOut ()
	{
		return Observable.Create<Unit> (observer => {
			_currentTween = UICanvasGroup.DOFade (0.0f, _transitionDuration)
				.OnStart (() => {
					UICanvasGroup.interactable = false;
				})
				.OnComplete (() => {
					gameObject.gameObject.SetActive (false);
					observer.OnNext (Unit.Default);
					observer.OnCompleted ();
				});

			return Disposable.Create (() => {
				if (_currentTween != null)
				{
					_currentTween.Kill ();
					_currentTween = null;
				}
			});
		});
	}
}
