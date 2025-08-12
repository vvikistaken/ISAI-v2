using Godot;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public partial class ChatBox : Panel
{
    private static string CurrentPath = Directory.GetCurrentDirectory();
    private RichTextLabel _chatLog;
    private LineEdit _userMessage;
    private Button _sendButton;

    [Export]
    public Ui UI_Ref;
    private Tuple<string, bool, double[]> modelParameters;

    public override void _Ready()
    {
        _chatLog = GetNode<RichTextLabel>("ChatLog");
        _userMessage = GetNode<LineEdit>("UserInput/UserMessage");
        _sendButton = GetNode<Button>("UserInput/SendButton");

        _sendButton.Pressed += OnSendButtonPressed;

        UI_Ref.ClearMemory += Clear_Memory;

        Load_Memory();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.Enter && _userMessage.HasFocus())
            {
                _userMessage.ReleaseFocus();
                OnSendButtonPressed();
            }
        }
    }

    public async void OnSendButtonPressed()
    {
        string message = _userMessage.Text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            // user side
            _chatLog.Text += $"\nUser: {message}\n";
            _userMessage.Clear();

            // model side
            ToggleUserInput(); //turn off
            string response = await Task.Run(() => GenerateResponse(message));
            ToggleUserInput(); //turn on
            _chatLog.Text += $"\nISAI: {response}\n";
        }
        // unnessesarry, there's already a function for that
        //_chatLog.ScrollToLine(_chatLog.GetLineCount() - 1);
    }

    private string GenerateResponse(string message)
    {
        var modelParameters = UI_Ref.GetModelParameters();
        GD.Print("Sending message to model...");
        Godot.Collections.Array output = [];
        OS.Execute(
            "Scripts/LocalEnv/bin/python",
            new string[]{
                $"Scripts/llm.py",
                modelParameters.Item1,
                message,
                Convert.ToString(modelParameters.Item2),
                Convert.ToString(modelParameters.Item3[0]),
                Convert.ToString(modelParameters.Item3[1]),
                Convert.ToString(modelParameters.Item3[2]),
                Convert.ToString(modelParameters.Item3[3]),
                Convert.ToString(modelParameters.Item3[4]),
                Convert.ToString(modelParameters.Item3[5]),
                Convert.ToString(modelParameters.Item3[6]),
            },
            output,
            readStderr: true
        );
        GD.Print("Received response from model!");
        // processing output
        string[] formattedOutput = output[0].ToString().Split('\n');

        return formattedOutput[formattedOutput.Length - 2];
    }

    private void Load_Memory()
    {
        string filePath = Path.Combine(CurrentPath, "memory.txt");
        if (File.Exists(filePath))
        {
            string[] fileContent = File.ReadAllText(filePath).Split(":end:");
            foreach (string line in fileContent)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (line.StartsWith("system:")) continue;

                    string formattedLine = string.Empty;
                    if (line.StartsWith("user:"))
                        formattedLine = line.Replace("user:", "User:");
                    if (line.StartsWith("assistant:"))
                        formattedLine = line.Replace("assistant:", "ISAI:");

                    _chatLog.Text += $"{formattedLine}\n";
                }
            }
        }
    }
    private void Clear_Memory()
    {
        string filePath = Path.Combine(CurrentPath, "memory.txt");

        File.Delete(filePath);

        _chatLog.Text = string.Empty;
    }

    private void ToggleUserInput()
    {
        _sendButton.Disabled = !_sendButton.Disabled;
        _userMessage.Editable = !_userMessage.Editable;

        if (_userMessage.Editable)
            _userMessage.PlaceholderText = "type your message here . . .";
        else
            _userMessage.PlaceholderText = "Waiting for response . . .";
    }
}
