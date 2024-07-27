using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Common.Math;
using System.Runtime.InteropServices;

namespace DrahsidLib;

/// <summary>
/// Enumeration representing the value which dictates the mode the camera is in.
/// </summary>
public enum CameraControlMode {
    FirstPerson = 0,
    ThirdPerson = 1
}

/// <summary>
/// Enumeration representing the currently active control type. There are more possible values than these, I just don't know what they do.
/// </summary>
public enum CameraControlType {
    FirstPerson = 0,
    Legacy = 1,
    Standard = 2
}

/// <summary>
/// Enumeration representing the selected movement mode within the game's config
/// </summary>
public enum MovementMode {
    Standard = 0,
    Legacy = 1,
    Count
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.CameraBase with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x110)]
public unsafe struct GameCameraBase {
    [FieldOffset(0x00)] public void** vtbl;
    [FieldOffset(0x10)] public FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera SceneCamera;
    [FieldOffset(0x60)] public float X;
    [FieldOffset(0x64)] public float Z;
    [FieldOffset(0x68)] public float Y;
    [FieldOffset(0x90)] public float LookAtX;
    [FieldOffset(0x94)] public float LookAtZ;
    [FieldOffset(0x98)] public float LookAtY;
    [FieldOffset(0x100)] public uint UnkUInt;
    [FieldOffset(0x108)] public uint UnkFlags;
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.Camera with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public unsafe struct GameCamera {
    [FieldOffset(0x00)] public GameCameraBase CameraBase;
    [FieldOffset(0x114)] public float Distance; // "CurrentZoom"
    [FieldOffset(0x118)] public float MinDistance; // "MinZoom"
    [FieldOffset(0x11C)] public float MaxDistance; // "MaxZoom"

    [FieldOffset(0x120)] public float FoV; // "CurrentFoV"
    [FieldOffset(0x124)] public float MinFoV;
    [FieldOffset(0x128)] public float MaxFoV;
    [FieldOffset(0x12C)] public float AddedFoV;

    [FieldOffset(0x130)] public float Yaw; // "CurrentHRotation"
    [FieldOffset(0x134)] public float Pitch; // "CurrentVRotation"
    [FieldOffset(0x138)] public float YawDelta; // "HRotationDelta"
    [FieldOffset(0x148)] public float MinPitch; // "MinVRotation", radians
    [FieldOffset(0x14C)] public float MaxPitch; // "MaxVRotation", radians
    [FieldOffset(0x160)] public float Roll; // "Tilt", radians

    [FieldOffset(0x170)] public int Mode; // CameraControlMode
    [FieldOffset(0x174)] public int ControlType; // CameraControlType

    [FieldOffset(0x17C)] public float InterpDistance; // "InterpolatedZoom"
    [FieldOffset(0x188)] public float SavedDistance;
    [FieldOffset(0x190)] public float Transition; // Seems to be related to the 1st <-> 3rd camera transition

    [FieldOffset(0x1B0)] public float ViewX;
    [FieldOffset(0x1B4)] public float ViewZ;
    [FieldOffset(0x1B8)] public float ViewY;

    [FieldOffset(0x1E4)] public byte IsFlipped;
}

[StructLayout(LayoutKind.Explicit)]
public struct GPoseCamera
{
    [FieldOffset(0x60)] public Vector3 Position;

    [FieldOffset(0x114)] public float Distance;
    [FieldOffset(0x118)] public float DistanceMin;
    [FieldOffset(0x11C)] public float DistanceMax;
    [FieldOffset(0x12C)] public float FoV;
    [FieldOffset(0x130)] public Vector2 Angle;
    [FieldOffset(0x150)] public Vector2 Pan;
    [FieldOffset(0x160)] public float Rotation;
    [FieldOffset(0x208)] public Vector2 DistanceCollide;
    [FieldOffset(0x14C)] public float YMin;
    [FieldOffset(0x148)] public float YMax;

    public Vector3 CalcRotation() => new(Angle.X - Pan.X, -Angle.Y - Pan.Y, Rotation);
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.LobbyCamera with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x300)]
public unsafe struct GameLobbyCamera {
    [FieldOffset(0x00)] public GameCamera Camera;
    [FieldOffset(0x2F8)] public void* LobbyExcelSheet;
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.Camera3 with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x300)]
public struct GameCamera3 {
    [FieldOffset(0x00)] public GameCamera Camera;
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.LowCutCamera with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x2E0)]
public struct GameLowCutCamera {
    [FieldOffset(0x00)] public GameCameraBase CameraBase;
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.Camera4 with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x350)]
public struct GameCamera4 {
    [FieldOffset(0x00)] public GameCameraBase CameraBase;

    [FieldOffset(0x110)] public FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera SceneCamera0;
    [FieldOffset(0x200)] public FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera SceneCamera1;
}

/// <summary>
/// FFXIVClientStructs.FFXIV.Client.Game.Control.GameCameraManager with some additional fields
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x180)]
public unsafe partial struct GameCameraManager {
    public static GameCameraManager* Instance() => (GameCameraManager*)Control.Instance();

    [FieldOffset(0x00)] public GameCamera* Camera; // "WorldCamera"
    [FieldOffset(0x08)] public GameLowCutCamera* LowCutCamera; // "IdleCamera"
    [FieldOffset(0x10)] public GameLobbyCamera* LobbCamera; // "MenuCamera"
    [FieldOffset(0x18)] public GameCamera3* Camera3; // "SpectatorCamera"
    [FieldOffset(0x20)] public GameCamera4* Camera4;

    [FieldOffset(0x48)] public int ActiveCameraIndex;
    [FieldOffset(0x4C)] public int PreviousCameraIndex;

    [FieldOffset(0x60)] public CameraBase UnkCamera; // not a pointer

    public GameCamera* GetActiveCamera() => (GameCamera*)CameraManager.Instance();
}


