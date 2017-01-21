using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	START_MENU 	= 0,
	PLAYING 	= 1,
	GAME_OVER 	= 2
}

public class GameManager : Singleton<GameManager>
{

}
