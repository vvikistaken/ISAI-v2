using Godot;
using System;

public partial class Isai : Node2D
{
    // will probably remake it
    // maybe in physics bones
    // or with normal bone but way better than this

    private Skeleton2D _skeleton;
    private Bone2D _baseBone, _leftShoulder, _rightShoulder, _head, _antennaLeft, _antennaRight;
    double time = 0;
    public override void _Ready()
    {
        _skeleton = GetNode<Skeleton2D>("Skeleton2D");
        _baseBone = _skeleton.GetNode<Bone2D>("Base");
        _leftShoulder = _skeleton.GetNode<Bone2D>("Base/Spine/Neck/LeftShoulder");
        _rightShoulder = _skeleton.GetNode<Bone2D>("Base/Spine/Neck/RightShoulder");
        _head = _skeleton.GetNode<Bone2D>("Base/Spine/Neck/Head");
        _antennaLeft = _head.GetNode<Bone2D>("AntennaL");
        _antennaRight = _head.GetNode<Bone2D>("AntennaR");
    }

    public override void _Process(double delta)
    {
        time++;

        _leftShoulder.Rotation = (float)Math.Sin(time * 0.01) * 0.1f;
        _rightShoulder.Rotation = (float)Math.Cos(time * 0.01) * 0.1f;
        _head.Rotation = (float)(Math.Sin(time * 0.005) * Math.Cos(time * 0.005)) * 0.25f;
        _antennaLeft.Rotation = (float)Math.Cos(time * 0.01) * 0.5f;
        _antennaRight.Rotation = (float)Math.Sin(time * 0.01) * 0.5f;

        _baseBone.MoveLocalX(
            0-(float)(Math.Sin(time * 0.005) * Math.Cos(time * 0.005)) * 0.05f
        );
    }
}
