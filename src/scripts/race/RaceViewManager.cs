using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static CharacterScript;

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
      var playerId = turnResult.Player.Id;
      var playerSprite = _characters.FirstOrDefault(c => c.Id == playerId);
      var moveXAmount = (RaceSceneData.SpaceWidth * turnResult.Movement);
      var newXPosition = moveXAmount + playerSprite.Position.x;
      if (playerId == GameManager.LocalPlayer.Id && newXPosition > _characterRightBound.x)
      {
        playerSprite.Move(_characterRightBound.x - playerSprite.Position.x);
        var extraMove = newXPosition - _characterRightBound.x;
        _tileMapManager.ScrollRight(extraMove);
        foreach (var otherCharacter in _characters.Where(c => c.Id != GameManager.LocalPlayer.Id))
        {
          otherCharacter.Move(-extraMove);
        }
      }
      else
      {
        playerSprite.Move(moveXAmount);
      }
    }
  }

  public void RaceEndAnimation()
  {
    foreach (var character in _characters)
    {
      character.RaceOverAnimation();
    }
  }
}