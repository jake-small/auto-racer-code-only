using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public class RaceViewManager
{
  private TileMapManager _tileMapManager;
  private List<CharacterScript> _characters;
  private Vector2 _characterLeftBound;
  private Vector2 _characterRightBound;


  public RaceViewManager(TileMapManager tileMapManager, IEnumerable<CharacterScript> characterScripts, Vector2 characterLeftBound, Vector2 characterRightBound)
  {
    _tileMapManager = tileMapManager;
    _characters = characterScripts.ToList();
    _characterLeftBound = characterLeftBound;
    _characterRightBound = characterRightBound;
  }

  public void MovePlayers(IEnumerable<PlayerTurnResult> turnResults)
  {
    foreach (var turnResult in turnResults)
    {
      MovePlayer(turnResult);
    }
  }

  private void MovePlayer(PlayerTurnResult turnResult)
  {
    var playerId = turnResult.Player.Id;
    var playerSprite = _characters.FirstOrDefault(c => c.Id == playerId);
    var newXPosition = playerSprite.Position.x + (RaceSceneData.SpaceWidth * turnResult.Movement);

    if (playerId == GameManager.LocalPlayer.Id && newXPosition > _characterRightBound.x)
    {
      playerSprite.Move(_characterRightBound.x);
      var extraMove = newXPosition - _characterRightBound.x;
      _tileMapManager.ScrollRight(extraMove);
    }
    else
    {
      playerSprite.Move(newXPosition);
    }
  }
}