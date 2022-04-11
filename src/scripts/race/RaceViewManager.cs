using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static CharacterScript;

public class RaceViewManager
{
  private TileMapManager _tileMapManager;
  private List<CharacterScript> _characters;
  private Vector2 _characterSoftLeftBound;
  private Vector2 _characterSoftRightBound;
  private Vector2 _characterHardLeftBound;
  private Vector2 _characterHardRightBound;


  public RaceViewManager(TileMapManager tileMapManager, IEnumerable<CharacterScript> characterScripts, Vector2 characterSoftLeftBound, Vector2 characterSoftRightBound, Vector2 characterHardLeftBound, Vector2 characterHardRightBound)
  {
    _tileMapManager = tileMapManager;
    _characters = characterScripts.ToList();
    _characterSoftLeftBound = characterSoftLeftBound;
    _characterSoftRightBound = characterSoftRightBound;
    _characterHardLeftBound = characterHardLeftBound;
    _characterHardRightBound = characterHardRightBound;
  }

  public void MovePlayers(IEnumerable<PlayerTurnResult> turnResults)
  {
    foreach (var turnResult in turnResults)
    {
      var playerId = turnResult.Player.Id;
      var playerSprite = _characters.FirstOrDefault(c => c.Id == playerId);
      var moveXAmount = (RaceSceneData.SpaceWidth * turnResult.Movement);
      var newXPosition = moveXAmount + playerSprite.Position.x;
      if (playerId == GameManager.LocalPlayer.Id
        && _characters.Any(c => c.Id != GameManager.LocalPlayer.Id && c.Position.x > playerSprite.Position.x))
      {
        TryScrollRight(playerSprite, newXPosition, _characterSoftLeftBound.x);
      }
      else if (playerId == GameManager.LocalPlayer.Id && newXPosition >= _characterSoftRightBound.x)
      {
        TryScrollRight(playerSprite, newXPosition, _characterSoftRightBound.x);
      }
      else if (playerId == GameManager.LocalPlayer.Id && newXPosition <= _characterSoftLeftBound.x)
      {
        TryScrollLeft(playerSprite, newXPosition, _characterSoftLeftBound.x);
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

  private bool TryScrollRight(CharacterScript playerSprite, float newXPosition, float boundPosition)
  {
    var extraMove = newXPosition - boundPosition;

    // only move if main character stays within soft bounds
    if (playerSprite.Id != GameManager.LocalPlayer.Id)
    {
      var localCharacter = _characters.FirstOrDefault(c => c.Id == GameManager.LocalPlayer.Id);
      if (localCharacter.Position.x - extraMove < _characterSoftLeftBound.x)
      {
        return false;
      }
    }

    _tileMapManager.ScrollRight(extraMove);
    foreach (var otherCharacter in _characters.Where(c => c.Id != playerSprite.Id))
    {
      otherCharacter.Move(-extraMove);
    }
    playerSprite.Move(boundPosition - playerSprite.Position.x);
    return true;
  }

  // TODO
  private bool TryScrollLeft(CharacterScript playerSprite, float newXPosition, float boundPosition)
  {
    var extraMove = boundPosition - newXPosition;
    _tileMapManager.ScrollLeft(extraMove);
    foreach (var otherCharacter in _characters.Where(c => c.Id != playerSprite.Id))
    {
      otherCharacter.Move(extraMove);
    }
    playerSprite.Move(playerSprite.Position.x - boundPosition);
    return true;
  }
}