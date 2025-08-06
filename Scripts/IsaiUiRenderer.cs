using Godot;
using System;

public partial class IsaiUiRenderer : Control
{
    private Isai _isai;
    private TextureRect _backgroundRect;
    private ResourcePreloader _preloader;

    [Export]
    public Ui UI_Ref;

    public override void _Ready()
    {
        _isai = GetNode<Isai>("ISAI");
        _backgroundRect = GetNode<TextureRect>("BackgroundRect");
        _preloader = GetNode<ResourcePreloader>("ResourcePreloader");

        UI_Ref.ChangeBackground += OnChangeBackground;
    }

    public override void _Process(double delta)
    {
        // Scaling ISAI based on parent size
        // Default: 768px --> x3 scale
        _isai.Scale = new Vector2(
            this.Size.Y / 256,
            this.Size.Y / 256
        );

        // Puts ISAI right in the center
        _isai.Position = new Vector2(
            this.Size.X / 2,
            this.Size.Y
        );
    }

    private void OnChangeBackground(string path)
    {
        if (path != string.Empty)
        {
            Image image = new Image();
            ImageTexture imageTexture = new ImageTexture();

            image.Load(path);
            imageTexture.SetImage(image);

            _backgroundRect.Texture = imageTexture;
        }
        else
        {
            Resource defaultBackground = _preloader.GetResource("default_background");
            _backgroundRect.Texture = (Texture2D)defaultBackground;
        }
    }
}
