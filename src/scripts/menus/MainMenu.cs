using Godot;
using System;

public class MainMenu : Control
{
  private CharacterScript _playerCharacter;
  private int _characterSelectIndex;
  private Label _firstNameLabel;
  private Label _adjectiveLabel;
  private TextureButton _vsBotsToggle;

  public override void _EnterTree()
  {
    base._EnterTree();
    GameManager.Reset();
    LoadCharacterSkins();
  }
  public override void _Ready()
  {
    _playerCharacter = GetNode<CharacterScript>(MainMenuData.PlayerCharacter);
    _characterSelectIndex = GameManager.CharacterSkins.FindIndex(s => s.Equals(_playerCharacter.CharacterSkin, StringComparison.InvariantCultureIgnoreCase));
    var firstNameRandomButton = GetNode<Button>(MainMenuData.ButtonRandomFirstName);
    firstNameRandomButton.Connect("pressed", this, nameof(Button_first_name_random_pressed));
    var adjectiveRandomButton = GetNode<Button>(MainMenuData.ButtonRandomAdjective);
    adjectiveRandomButton.Connect("pressed", this, nameof(Button_adjective_random_pressed));
    var nameRandomButton = GetNode<TextureButton>(MainMenuData.ButtonRandomName);
    nameRandomButton.Connect("pressed", this, nameof(Button_name_random_pressed));
    var startFfaButton = GetNode<TextureButton>(MainMenuData.ButtonStartFFA);
    startFfaButton.Connect("pressed", this, nameof(Button_start_ffa_pressed));
    var start1v1Button = GetNode<TextureButton>(MainMenuData.ButtonStart1v1);
    start1v1Button.Connect("pressed", this, nameof(Button_start_1v1_pressed));
    _vsBotsToggle = GetNode<TextureButton>(MainMenuData.ButtonToggleVsBots);
    // var quitButton = GetNode<TextureButton>(MainMenuData.ButtonQuit);
    // quitButton.Connect("pressed", this, nameof(Button_quit_pressed));

    var creditsButton = GetNode<TextureButton>(MainMenuData.ButtonCredits);
    creditsButton.Connect("pressed", this, nameof(Button_credits_pressed));
    var prevSkinButton = GetNode<TextureButton>(MainMenuData.ButtonSkinPrevious);
    prevSkinButton.Connect("pressed", this, nameof(Button_previous_skin_pressed));
    var nextSkinButton = GetNode<TextureButton>(MainMenuData.ButtonSkinNext);
    nextSkinButton.Connect("pressed", this, nameof(Button_next_skin_pressed));

    if (GameManager.NameGenerator is null)
    {
      GameManager.NameGenerator = new NameGenerator(PrepSceneData.NameDataRelativePath);
    }
    _firstNameLabel = GetNode<Label>(MainMenuData.LabelFirstName);
    _adjectiveLabel = GetNode<Label>(MainMenuData.LabelAdjective);
    _firstNameLabel.Text = GameManager.NameGenerator.GetRandomFirstName();
    _adjectiveLabel.Text = GameManager.NameGenerator.GetRandomAdjective();
  }

  private void Button_previous_skin_pressed()
  {
    Console.WriteLine("Previous Skin button pressed");
    _characterSelectIndex = _characterSelectIndex - 1;
    if (_characterSelectIndex < 0)
    {
      _characterSelectIndex = GameManager.CharacterSkins.Count - 1;
    }
    var prevSkin = GameManager.CharacterSkins[_characterSelectIndex];
    _playerCharacter.CharacterSkin = prevSkin;
  }

  private void Button_next_skin_pressed()
  {
    Console.WriteLine("Next Skin button pressed");
    _characterSelectIndex = _characterSelectIndex + 1;
    if (_characterSelectIndex >= GameManager.CharacterSkins.Count)
    {
      _characterSelectIndex = 0;
    }
    var nextSkin = GameManager.CharacterSkins[_characterSelectIndex];
    _playerCharacter.CharacterSkin = nextSkin;
  }

  private void Button_name_random_pressed()
  {
    Console.WriteLine("Random Name button pressed");
    _firstNameLabel.Text = GameManager.NameGenerator.GetRandomFirstName();
    _adjectiveLabel.Text = GameManager.NameGenerator.GetRandomAdjective();
  }

  private void Button_first_name_random_pressed()
  {
    Console.WriteLine("Random FirstName button pressed");
    _firstNameLabel.Text = GameManager.NameGenerator.GetRandomFirstName();
  }

  private void Button_adjective_random_pressed()
  {
    Console.WriteLine("Random Adjective button pressed");
    _adjectiveLabel.Text = GameManager.NameGenerator.GetRandomAdjective();
  }

  private void Button_start_ffa_pressed()
  {
    Console.WriteLine("Start FFA button pressed");
    GameManager.NumPlayers = 4;
    StartGame();
  }

  private void Button_start_1v1_pressed()
  {
    Console.WriteLine("Start 1v1 button pressed");
    GameManager.NumPlayers = 2;
    StartGame();
  }

  private void Button_quit_pressed()
  {
    Console.WriteLine("Quit button pressed");
    GetTree().Quit();
  }

  private void Button_credits_pressed()
  {
    Console.WriteLine("Credits button pressed");
    GetTree().ChangeScene(MainMenuData.CreditsScenePath);
  }

  private void StartGame()
  {
    GameManager.VsBots = _vsBotsToggle.Pressed;
    var playerName = $"{_firstNameLabel.Text} the {_adjectiveLabel.Text}";
    GameManager.LocalPlayer = new Player
    {
      Id = 0,
      Name = playerName,
      Cards = GameManager.PrepEngine.PlayerInventory.GetCards(),
      Position = 0
    };
    GameManager.LocalPlayer.Skin = _playerCharacter.CharacterSkin;
    GameManager.Score = new Score(GameManager.NumPlayers);
    GetTree().ChangeScene(MainMenuData.PrepScenePath);
  }

  private void LoadCharacterSkins()
  {
    var skinDirectory = MainMenuData.SkinDirectory;
    var directory = new Directory();
    if (directory.Open(skinDirectory) == Error.Ok)
    {
      directory.ListDirBegin();
      var skin = directory.GetNext();
      while (!String.IsNullOrWhiteSpace(skin))
      {
        if (!directory.CurrentIsDir())
        {
          GameManager.CharacterSkins.Add(System.IO.Path.Combine(skinDirectory, skin));
        }
        skin = directory.GetNext();
      }
      if (GameManager.CharacterSkins.Count <= 0)
      {
        GD.Print($"No character skins found at path: {skinDirectory}");
        throw new Exception($"No character skins found at path: {skinDirectory}");
      }
    }
    else
    {
      GD.Print($"Unable open the character skin directory at path: {skinDirectory}");
      throw new Exception($"Unable open the character skin directory at path: {skinDirectory}");
    }
  }
}
