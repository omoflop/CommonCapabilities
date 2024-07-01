using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace Shared;

public class Camera {
    private readonly OrthographicCamera _ortCam;
    public readonly ViewportAdapter View;

    public Camera(GameWindow window, GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight) {
        View = new BoxingViewportAdapter(window, graphicsDevice, virtualWidth, virtualHeight);
        _ortCam = new OrthographicCamera(View);
    }

    public Vector2 Position {
        get => _ortCam.Position;
        set => _ortCam.Position = value;
    }

    public Vector2 Origin {
        get => _ortCam.Origin;
        set => _ortCam.Origin = value;
    }

    public float Angle {
        get => _ortCam.Rotation * 180f / MathF.PI;
        set => _ortCam.Rotation = value * MathF.PI / 180f;
    }

    public float Zoom {
        get => _ortCam.Zoom;
        set => _ortCam.Zoom = value;
    }

    public float MaxZoom {
        get => _ortCam.MaximumZoom;
        set => _ortCam.MaximumZoom = value;
    }

    public float MinZoom {
        get => _ortCam.MinimumZoom;
        set => _ortCam.MinimumZoom = value;
    }

    public float Width => _ortCam.BoundingRectangle.Width;
    public float Height => _ortCam.BoundingRectangle.Height;
    public Vector2 Size => new(Width, Height);
    public RectangleF Bounds => _ortCam.BoundingRectangle;

    public void LookAt(Vector2 pos) {
        _ortCam.LookAt(pos);
    }

    public void Move(Vector2 dir) {
        _ortCam.Move(dir);
    }

    public void Rotate(float dDeg) {
        _ortCam.Rotate(dDeg * MathF.PI / 180f);
    }

    public void ZoomIn(float dZoom) {
        _ortCam.ZoomIn(dZoom);
    }

    public void ZoomOut(float dZoom) {
        _ortCam.ZoomOut(dZoom);
    }

    public Vector2 ScreenToWorld(Vector2 pos) {
        return _ortCam.ScreenToWorld(pos.X, pos.Y);
    }

    public Vector2 WorldToScreen(Vector2 pos) {
        return _ortCam.WorldToScreen(pos.X, pos.Y);
    }

    public Matrix GetViewMatrix() {
        return _ortCam.GetViewMatrix();
    }

    public BoundingFrustum GetBoundingFrustum() {
        return _ortCam.GetBoundingFrustum();
    }

    public Matrix GetInverseViewMatrix() {
        return _ortCam.GetInverseViewMatrix();
    }
}