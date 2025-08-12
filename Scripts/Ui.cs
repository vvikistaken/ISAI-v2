using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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
    // model setting
    CheckButton _gpuCheckButton;
    SpinBox _threadSpinBox, _tokenSpinBox, _temperatureSpinBox,
     _minPSpinBox, _topPSpinBox, _typicalPSpinBox, _topKSpinBox;
    Button _threadReset, _tokenReset, _temperatureReset, _minPReset,
    _topPReset, _typicalPReset, _topKReset ;

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
        // model setting
        _gpuCheckButton = _configPanel.GetNode<CheckButton>("Options/UseGPU/GPUCheckButton");
            // spinbox
        _threadSpinBox = _configPanel.GetNode<SpinBox>("Options/ThreadCount/ThreadSpinBox");
        _tokenSpinBox = _configPanel.GetNode<SpinBox>("Options/MaxTokens/TokenSpinBox");
        _temperatureSpinBox = _configPanel.GetNode<SpinBox>("Options/Temperature/TemperatureSpinBox");
        _minPSpinBox = _configPanel.GetNode<SpinBox>("Options/Top&MinP/MinP/MinPSpinBox");
        _topPSpinBox = _configPanel.GetNode<SpinBox>("Options/Top&MinP/TopP/TopPSpinBox");
        _typicalPSpinBox = _configPanel.GetNode<SpinBox>("Options/TypicalP&TopK/TypicalP/TypicalPSpinBox");
        _topKSpinBox = _configPanel.GetNode<SpinBox>("Options/TypicalP&TopK/TopK/TopKSpinBox");
            // reset buttons
        _threadReset = _configPanel.GetNode<Button>("Options/ThreadCount/ResetButton");
        _tokenReset = _configPanel.GetNode<Button>("Options/MaxTokens/ResetButton");
        _temperatureReset = _configPanel.GetNode<Button>("Options/Temperature/ResetButton");
        _minPReset = _configPanel.GetNode<Button>("Options/Top&MinP/MinP/ResetButton");
        _topPReset = _configPanel.GetNode<Button>("Options/Top&MinP/TopP/ResetButton");
        _typicalPReset = _configPanel.GetNode<Button>("Options/TypicalP&TopK/TypicalP/ResetButton");
        _topKReset = _configPanel.GetNode<Button>("Options/TypicalP&TopK/TopK/ResetButton");

        // signals
        _configButton.Pressed += OnConfigButtonPressed;
        // general signals
        _loadModelButton.Pressed += () => { _modelPathFileDialog.Visible = true; };
        _modelPathFileDialog.FileSelected += (path) =>
        {
            _pathPointer.Text = path;
            SaveConfiguration();
        };
        _resetBackgroundButton.Pressed += () =>
        {
            _backgroundPointer.Text = string.Empty;
            EmitSignal(SignalName.ChangeBackground, string.Empty);
            SaveConfiguration();
        };
        _loadBackgroundButton.Pressed += () => { _backgroundFileDialog.Visible = true; };
        _backgroundFileDialog.FileSelected += (path) =>
        {
            _backgroundPointer.Text = path;
            EmitSignal(SignalName.ChangeBackground, path);
            SaveConfiguration();
        };
        _clearButton.Pressed += () =>
        {
            _clearMemoryDialog.Visible = true;
        };
        _clearMemoryDialog.Confirmed += () =>
        {
            EmitSignal(SignalName.ClearMemory);
            ToggleConfigPanel();
        };
        // model signals
        _gpuCheckButton.Toggled += (toggled) =>
        {
            SaveConfiguration();
        };
        SetupModelSettings(_threadSpinBox, _threadReset);
        SetupModelSettings(_tokenSpinBox, _tokenReset);
        SetupModelSettings(_temperatureSpinBox, _temperatureReset);
        SetupModelSettings(_minPSpinBox, _minPReset);
        SetupModelSettings(_topPSpinBox, _topPReset);
        SetupModelSettings(_typicalPSpinBox, _typicalPReset);
        SetupModelSettings(_topKSpinBox, _topKReset);

        // methods

        ToggleConfigPanel(); // Initialize the config panel state

        LoadConfiguration(); // load in config settings
    }

    private void OnConfigButtonPressed()
    {
        _configPanel.Visible = !_configPanel.Visible;
    }

    private void SetupModelSettings(SpinBox spinBox, Button resetButton)
    {
        double defaultValue = spinBox.Value;

        resetButton.Pressed += () =>
        {
            spinBox.Value = defaultValue;
            SaveConfiguration();
        };
        spinBox.ValueChanged += (value) =>
        {
            SaveConfiguration();
        };
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

        // general
        config.SetValue("General", "ModelPath", _pathPointer.Text);
        config.SetValue("General", "BackgroundImage", _backgroundPointer.Text);
        // model
        config.SetValue("Model", "UseGPU", _gpuCheckButton.ButtonPressed);
        config.SetValue("Model", "ThreadsUsed", _threadSpinBox.Value);
        config.SetValue("Model", "MaxTokens", _tokenSpinBox.Value);
        config.SetValue("Model", "Temperature", _temperatureSpinBox.Value);
        config.SetValue("Model", "MinP", _minPSpinBox.Value);
        config.SetValue("Model", "TopP", _topPSpinBox.Value);
        config.SetValue("Model", "TypicalP", _typicalPSpinBox.Value);
        config.SetValue("Model", "TopK", _topKSpinBox.Value);

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

        // general
        _pathPointer.Text = (string)config.GetValue("General", "ModelPath");
        _backgroundPointer.Text = (string)config.GetValue("General", "BackgroundImage");
        // model
        _gpuCheckButton.ButtonPressed = (bool)config.GetValue("Model", "UseGPU", false);
        _threadSpinBox.Value = (int)config.GetValue("Model", "ThreadsUsed", 1);
        _tokenSpinBox.Value = (int)config.GetValue("Model", "MaxToken", 512);
        _temperatureSpinBox.Value = (double)config.GetValue("Model", "Temperature", 1.0);
        _minPSpinBox.Value = (double)config.GetValue("Model", "MinP", 0.05);
        _topPSpinBox.Value = (double)config.GetValue("Model", "TopP", 0.95);
        _typicalPSpinBox.Value = (double)config.GetValue("Model", "TypicalP", 1.0);
        _topKSpinBox.Value = (int)config.GetValue("Model", "TopK", 40);
    }
}
