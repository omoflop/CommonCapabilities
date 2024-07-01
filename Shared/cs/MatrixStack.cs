using Microsoft.Xna.Framework;

namespace Shared;

public ref struct MatrixStack() {
    public Matrix Top = Matrix.Identity;
    private Matrix[] _stack = new Matrix[4];
    private int _count = 0;

    public void LoadIdentity() {
        _count = 0;
        Top = Matrix.Identity;
    }

    public void Push() {
        if (_count == _stack.Length) {
            // Resize the array if needed
            Array.Resize(ref _stack, _stack.Length * 2);
        }
        _stack[_count++] = Top;
    }

    public void Pop() {
        if (_count == 0) {
            throw new InvalidOperationException("Cannot pop from an empty stack.");
        }
        Top = _stack[--_count];
    }

    public void Multiply(Matrix mat) {
        Matrix.Multiply(ref Top, ref mat, out Top);
    }

    public void MultiplyLocal(Matrix mat) {
        Matrix.Multiply(ref mat, ref Top, out Top);
    }

    public void RotateAxis(Vector3 axis, float angle) {
        Matrix.CreateFromAxisAngle(ref axis, angle, out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void RotateAxisLocal(Vector3 axis, float angle) {
        Matrix.CreateFromAxisAngle(ref axis, angle, out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }

    public void Rotate(float yaw, float pitch, float roll) {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        Matrix.CreateFromQuaternion(ref q, out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void RotateDegrees(float yaw, float pitch, float roll) {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
        Matrix.CreateFromQuaternion(ref q, out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void RotateLocal(float pitch, float yaw, float roll) {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        Matrix.CreateFromQuaternion(ref q, out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }

    public void RotateLocalDegrees(float pitch, float yaw, float roll) {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
        Matrix.CreateFromQuaternion(ref q, out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }

    public void RotateAxisDegrees(Vector3 axis, float angle) {
        Matrix.CreateFromAxisAngle(ref axis, MathHelper.ToRadians(angle), out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void RotateAxisLocalDegrees(Vector3 axis, float angle) {
        Matrix.CreateFromAxisAngle(ref axis, MathHelper.ToRadians(angle), out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }

    public void Scale(float x, float y, float z) {
        Matrix.CreateScale(x, y, z, out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void ScaleLocal(float x, float y, float z) {
        Matrix.CreateScale(x, y, z, out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }

    public void Translate(float x, float y, float z) {
        Matrix.CreateTranslation(x, y, z, out Matrix tmp);
        Matrix.Multiply(ref Top, ref tmp, out Top);
    }

    public void TranslateLocal(float x, float y, float z) {
        Matrix.CreateTranslation(x, y, z, out Matrix tmp);
        Matrix.Multiply(ref tmp, ref Top, out Top);
    }
}