using Godot;
using System;

public partial class Ui : Control
{
    [Signal]
    public delegate void ChangeBackgroundEventHandler(string imagePath);
    [Signal]
    public delegate void ClearMemoryEventHandler();
    Button _configButton, _loadModelButton, _loadBackgroundButton, _resetBackgroundButton, _clearButton;
    Panel _configPanel;
    LineEdit _pathPointer, _backgroundPointer;
    FileDialog _modelPathFileDialog, _backgroundFileDialog;
    HBoxContainer _pathToModel, _backgroundOptions, _memoryOptions;
    ConfirmationDialog _clearMemoryDialog;

    public override void _Ready()
    {
        _configButton = GetNode<Button>("ConfigButton");
        _configPanel = GetNode<Panel>("ConfigPanel");
        // path to model
        _pathToModel = _configPanel.GetNode<HBoxContainer>("Options/PathToModel");
        _pathPointer = _pathToModel.GetNode<LineEdit>("ScrollContainer/PathPointer");
        _loadModelButton = _pathToModel.GetNode<Button>("LoadPathButton");
        _modelPathFileDialog = GetNode<FileDialog>("ModelPathFileDialog");
        // ISAI background
        _backgroundOptions = _configPanel.GetNode<HBoxContainer>("Options/BackgroundOptions");
        _backgroundPointer = _backgroundOptions.GetNode<LineEdit>("ScrollContainer/BackgroundPointer");
        _resetBackgroundButton = _backgroundOptions.GetNode<Button>("ResetPathButton");
        _loadBackgroundButton = _backgroundOptions.GetNode<Button>("LoadPathButton");
        _backgroundFileDialog = GetNode<FileDialog>("BackgroundFileDialog");
        // memory options
        _memoryOptions = _configPanel.GetNode<HBoxContainer>("Options/MemoryOptions");
        _clearButton = _memoryOptions.GetNode<Button>("ClearButton");
        _clearMemoryDialog = GetNode<ConfirmationDialog>("ClearMemoryDialog");

        // signals
        _configButton.Pressed += OnConfigButtonPressed;
        // path to model - signals
        _loadModelButton.Pressed += () => { _modelPathFileDialog.Visible = true; };
        _modelPathFileDialog.FileSelected += (path) => {
            _pathPointer.Text = path;
            SaveConfiguration();
        };
        // ISAI background - signals
        _resetBackgroundButton.Pressed += () => {
            _backgroundPointer.Text = string.Empty;
            EmitSignal(SignalName.ChangeBackground, string.Empty);
            SaveConfiguration();
        };
        _loadBackgroundButton.Pressed += () => { _backgroundFileDialog.Visible = true; };
        _backgroundFileDialog.FileSelected += (path) => {
            _backgroundPointer.Text = path;
            EmitSignal(SignalName.ChangeBackground, path);
            SaveConfiguration();
        };
        // memory options - signals
        _clearButton.Pressed += () => {
            _clearMemoryDialog.Visible = true;
        };
        _clearMemoryDialog.Confirmed += () =>
        {
            EmitSignal(SignalName.ClearMemory);
            ToggleConfigPanel();
        };

        // methods

        ToggleConfigPanel(); // Initialize the config panel state

        LoadConfiguration(); // load in config settings
    }

    private void OnConfigButtonPressed()
    {
        _configPanel.Visible = !_configPanel.Visible;
    }

    private void ToggleConfigPanel()
    {
        if (_configPanel.Visible)
            _configPanel.MouseFilter = MouseFilterEnum.Stop;
        else
            _configPanel.MouseFilter = MouseFilterEnum.Ignore;

        _configPanel.Visible = !_configPanel.Visible;
    }
    // full save
    private void SaveConfiguration()
    {
        ConfigFile config = new ConfigFile();

        config.SetValue("General", "ModelPath", _pathPointer.Text);
        config.SetValue("General", "BackgroundImage", _backgroundPointer.Text);

        var err = config.Save("user://config.cfg");
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save config file: {err}");
            return;
        }
    }
    // overload for saving single
    // nvm, it doesn't work
    private void SaveConfiguration(
        bool modelPath = false,
        bool backgroundImage = false
    )
    {
        ConfigFile config = new ConfigFile();

        if (modelPath)
            config.SetValue("General", "ModelPath", _pathPointer.Text);
        if (backgroundImage)
            config.SetValue("General", "BackgroundImage", _backgroundPointer.Text);

        var err = config.Save("user://config.cfg");
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save config file: {err}");
            return;
        }
    }
    private void LoadConfiguration()
    {
        ConfigFile config = new ConfigFile();
        var err = config.Load("user://config.cfg");
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load config file: {err}");
            return;
        }

        _pathPointer.Text = (string)config.GetValue("General", "ModelPath");
        _backgroundPointer.Text = (string)config.GetValue("General", "BackgroundImage");
    }
}
