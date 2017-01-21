using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
	[SerializeField]
	private GameObject _pooledGameObject			= null;
	[SerializeField]
	private Transform _pooledGameObjectContainer 	= null;
	[SerializeField]
	private int _numPooledObjects					= 10;
	[SerializeField]
	private bool _willGrow							= true;
	[SerializeField]
	private bool _populatePoolAtStart 				= true;

	private List<GameObject> _pooledGameObjects;

	private void Start ()
	{
		_pooledGameObjects = new List<GameObject> ();

		if (_populatePoolAtStart)
		{
			for (int i = 0; i < _numPooledObjects; ++i)
			{
				AddGameObjectToPool ();
			}
		}
	}

	private GameObject AddGameObjectToPool ()
	{
		GameObject go = Instantiate (_pooledGameObject, _pooledGameObjectContainer) as GameObject;
		go.SetActive (false);
		_pooledGameObjects.Add (go);
		return go;
	}

	public GameObject GetPooledObject ()
	{
		int numPooledObjects = _pooledGameObjects.Count;

		for (int i = 0; i < numPooledObjects; ++i)
		{
			if (!_pooledGameObjects[i].activeInHierarchy)
			{
				return _pooledGameObjects[i];
			}
		}

		if (_willGrow)
		{
			return AddGameObjectToPool ();
		}

		return null;
	}
}
