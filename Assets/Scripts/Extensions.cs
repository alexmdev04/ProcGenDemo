using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public static class Extensions
{
    const string
        ext_timeDays = "d ",
        ext_timeHours = "h ",
        ext_timeMinutes = "m ",
        ext_timeSeconds = "s ",
        ext_comma = ", ",
        ext_leftBracket = "(",
        ext_rightBracket = ")",
        ext_notApplicable = "n/a",
        ext_zeroSec = "0s";

    readonly static Dictionary<Key, string> keyStrings = new()
    {
        { Key.None, "" },
        { Key.Space, "␣" },
        { Key.Backquote, "`" },
        { Key.Quote, "'" },
        { Key.Semicolon, ";" },
        { Key.Comma, "," },
        { Key.Period, "." },
        { Key.Slash, "/" },
        { Key.Backslash, "\\" },
        { Key.LeftBracket, "(" },
        { Key.RightBracket, ")" },
        { Key.Minus, "-" },
        { Key.Equals, "=" },
        { Key.Digit1, "1" },
        { Key.Digit2, "2" },
        { Key.Digit3, "3" },
        { Key.Digit4, "4" },
        { Key.Digit5, "5" },
        { Key.Digit6, "6" },
        { Key.Digit7, "7" },
        { Key.Digit8, "8" },
        { Key.Digit9, "9" },
        { Key.Digit0, "0" },
        { Key.LeftShift, "Left Shift" },
        { Key.RightShift, "Right Shift" },
        { Key.LeftAlt, "Left Alt" },
        { Key.RightAlt, "Right Alt" },
        { Key.LeftCtrl, "Left Ctrl" },
        { Key.RightCtrl, "Right Ctrl" },
        { Key.LeftMeta, "Win" },
        { Key.RightMeta, "Win" },
        { Key.ContextMenu, "Context Menu" },
        { Key.Escape, "Esc" },
        { Key.LeftArrow, "←" },
        { Key.RightArrow, "→" },
        { Key.UpArrow, "↑" },
        { Key.DownArrow, "↓" },
        { Key.Backspace, "Back Space" },
        { Key.PageDown, "Page Down" },
        { Key.PageUp, "Page Up" },
        { Key.Insert, "Ins" },
        { Key.Delete, "Del" },
        { Key.CapsLock, "Caps Lock" },
        { Key.NumLock, "Num Lock" },
        { Key.PrintScreen, "Print Screen" },
        { Key.ScrollLock, "Scroll Lock" },
        { Key.NumpadEnter, "Num Enter" },
        { Key.NumpadDivide, "Num /" },
        { Key.NumpadMultiply, "Num *" },
        { Key.NumpadPlus, "Num +" },
        { Key.NumpadMinus, "Num -" },
        { Key.NumpadPeriod, "Num ." },
        { Key.NumpadEquals, "Num =" },
        { Key.Numpad0, "Num 0" },
        { Key.Numpad1, "Num 1" },
        { Key.Numpad2, "Num 2" },
        { Key.Numpad3, "Num 3" },
        { Key.Numpad4, "Num 4" },
        { Key.Numpad5, "Num 5" },
        { Key.Numpad6, "Num 6" },
        { Key.Numpad7, "Num 7" },
        { Key.Numpad8, "Num 8" },
        { Key.Numpad9, "Num 9" },
    };
    /// <summary>
    /// Sets this transform and all its children to the specified layer
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="layer"></param>
    public static void SetChildLayers(this Transform t, int layer)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            child.gameObject.layer = layer;
            SetChildLayers(child, layer);
        }
    }
    public static Quaternion ReflectRotation(this Quaternion source, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
    }
    /// <summary>
    /// Converts a time given in seconds to days, hours, minutes and seconds (e.g. 1d 19h 35m 7s)
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string ConvertTime(this double seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds((int)seconds);
        return
            (ts.Days > 0 ? ts.Days.ToString() + ext_timeDays : string.Empty) +
            (ts.Hours > 0 ? ts.Hours.ToString() + ext_timeHours : string.Empty) +
            (ts.Minutes > 0 ? ts.Minutes.ToString() + ext_timeMinutes : string.Empty) +
            (ts.Seconds > 0 ? ts.Seconds.ToString() + ext_timeSeconds : ext_zeroSec);
    }
    /// <summary>
    /// Calculates the greatest common denominator of a and b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float CalcGCD(float a, float b)
    {
        while (a != 0f && b != 0f)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }
        if (a == 0f)
            return b;
        else
            return a;
    }
    /// <summary>
    /// Converts a Vector3 to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector3 vector3)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector3.x.ToString()).Append(ext_comma)
            .Append(vector3.y.ToString()).Append(ext_comma)
            .Append(vector3.z.ToString()).Append(ext_rightBracket);
    }
        /// <summary>
    /// Converts a Vector3Int to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector3Int vector3)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector3.x.ToString()).Append(ext_comma)
            .Append(vector3.y.ToString()).Append(ext_comma)
            .Append(vector3.z.ToString()).Append(ext_rightBracket);
    }
    /// <summary>
    /// Converts a Vector3 to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector2 vector2)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector2.x.ToString()).Append(ext_comma)
            .Append(vector2.y.ToString()).Append(ext_rightBracket);
    }
    /// <summary>
    /// Converts a Vector3 to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector2Int vector2)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector2.x.ToString()).Append(ext_comma)
            .Append(vector2.y.ToString()).Append(ext_rightBracket);
    }
    /// <summary>
    /// Converts an array of Colliders to a StringBuilder containing a comma-separated list of all the Collider's GameObject's names
    /// </summary>
    /// <param name="colliders"></param>
    /// <param name="ifNullText"></param>
    /// <param name="ifEmptyText"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Collider[] colliders, string ifNullText = ext_notApplicable, string ifEmptyText = ext_notApplicable)
    {
        StringBuilder a = new();
        if (colliders == null) { return a.Append(ifNullText); }
        if (colliders.Length == 0) { return a.Append(ifEmptyText); }
        foreach (Collider collider in colliders) { if (collider != null) { a.Append(collider.gameObject.name).Append(ext_comma); } }
        return a;
    }
    public static bool AllCharsAreDigits(this char[] chars)
    {
        List<bool> isDigitBools = new();
        foreach (char _char in chars) { isDigitBools.Add(char.IsDigit(_char)); }
        return isDigitBools.All(x => x);
    }
    /// <summary>
    /// Converts a horizontal field of view value to a vertical field of view value (viewportHeight == Camera.pixelHeight)
    /// </summary>
    /// <param name="horizontalFOV"></param>
    /// <param name="viewportHeight"></param>
    /// <param name="viewportWidth"></param>
    /// <param name="clampFOV1to179"></param>
    /// <returns>
    /// Vertical field of view value
    /// </returns>
    public static float FOVHorizontalToVertical(float horizontalFOV, float viewportHeight, float viewportWidth, bool clampFOV1to179 = true)
    {
        return 2 * MathF.Atan(MathF.Tan((clampFOV1to179 ? Math.Clamp(horizontalFOV, 1, 179) : horizontalFOV) * MathF.PI / 180 / 2) * viewportHeight / viewportWidth) * 180 / MathF.PI;
    }
    /// <summary>
    /// Converts a horizontal field of view value to a vertical field of view value
    /// </summary>
    /// <param name="horizontalFOV"></param>
    /// <param name="camera"></param>
    /// <param name="clampFOV1to179"></param>
    /// <returns>
    /// Vertical field of view value
    /// </returns>
    public static float FOVHorizontalToVertical(float horizontalFOV, Camera camera, bool clampFOV1to179 = true)
    {
        return 2 * MathF.Atan(MathF.Tan((clampFOV1to179 ? Math.Clamp(horizontalFOV, 1, 179) : horizontalFOV) * MathF.PI / 180 / 2) * camera.pixelHeight / camera.pixelWidth) * 180 / MathF.PI;
    }
    public static float FOVHorizontalToVertical1(this float horizontalFOV, Camera camera, bool clampFOV1to179 = true)
    {
        return 2 * MathF.Atan(MathF.Tan((clampFOV1to179 ? Math.Clamp(horizontalFOV, 1, 179) : horizontalFOV) * MathF.PI / 180 / 2) * camera.pixelHeight / camera.pixelWidth) * 180 / MathF.PI;
    }
    public static float FOVVerticalToHorizontal(float verticalFOV, Camera camera, bool clampFOV1to179 = true)
    {
        return Mathf.Rad2Deg * (2 * MathF.Atan(MathF.Tan(((clampFOV1to179 ? Math.Clamp(verticalFOV, 1, 179) : verticalFOV) * Mathf.Deg2Rad) / 2) * camera.aspect));
    }
    public static float FOVVerticalToHorizontal1(this float verticalFOV, Camera camera, bool clampFOV1to179 = true)
    {
        return Mathf.Rad2Deg * (2 * MathF.Atan(MathF.Tan(((clampFOV1to179 ? Math.Clamp(verticalFOV, 1, 179) : verticalFOV) * Mathf.Deg2Rad) / 2) * camera.aspect));
    }
    public static Vector3 DirectionFromAngleY(float eulerY, float angleInDegrees) 
    {
        angleInDegrees += eulerY;
        return new Vector3(MathF.Sin(angleInDegrees * Mathf.Deg2Rad), 0, MathF.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    public static Vector3 DirectionFromAngleX(float eulerX, float angleInDegrees) 
    {
        angleInDegrees += eulerX;
        return new Vector3(0, MathF.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }
    public static Vector3 Round(this Vector3 vector3, int digits = 0)
    {
        return new Vector3(MathF.Round(vector3.x, digits), MathF.Round(vector3.y, digits), MathF.Round(vector3.z, digits));
    }
    public static float FloatFromAxis(bool positive, bool negative) =>
    (positive, negative) switch
    {
        (true, false) => 1f,
        (false, true) => -1f,
        _ => 0f
    };
    public static Vector3 Clamp(this Vector3 vector, float clamp)
    {
        return new Vector3(Math.Clamp(vector.x, -clamp, clamp), Math.Clamp(vector.y, -clamp, clamp), Math.Clamp(vector.z, -clamp, clamp));
    }
    public static Vector3 Clamp(this Vector3 vector, float clampMin, float clampMax)
    {
        return new Vector3(Math.Clamp(vector.x, clampMin, clampMax), Math.Clamp(vector.y, clampMin, clampMax), Math.Clamp(vector.z, clampMin, clampMax));
    }
    public static Vector2 Clamp(this Vector2 vector, float clamp)
    {
        return new Vector2(Math.Clamp(vector.x, -clamp, clamp), Math.Clamp(vector.y, -clamp, clamp));
    }
    public static Vector2 Clamp(this Vector2 vector, float clampMin, float clampMax)
    {
        return new Vector2(Math.Clamp(vector.x, clampMin, clampMax), Math.Clamp(vector.y, clampMin, clampMax));
    }
    public static Vector3 Add(this Vector3 vector, float number)
    {
        return new Vector3(vector.x + number, vector.y + number, vector.z + number);
    }
    public static string ToStringQuoted(this List<string> list)
    {
        StringBuilder output = new();
        foreach (string text in list) { output.Append("\"").Append(text).Append("\",\n"); }
        return output.ToString();
    }
    public static Vector2 GetPositionOfLastLetter(this TMPro.TextMeshProUGUI tmp_text)
    {
        tmp_text.ForceMeshUpdate();
        Vector3[] vertices = tmp_text.mesh.vertices;
        TMPro.TMP_CharacterInfo charInfo = tmp_text.textInfo.characterInfo[tmp_text.textInfo.characterCount - 1];
        return new Vector2((vertices[charInfo.vertexIndex + 0].x + vertices[charInfo.vertexIndex + 2].x) / 2, (charInfo.bottomLeft.y + charInfo.topLeft.y) / 2);
    }
    public static string ToStringTranslate(this Key key)
    {
        return keyStrings.TryGetValue(key, out string text) ? text : key.ToString();
    }
    public static Game.Directions EulerToCardinal(float eulerY) 
    {
        return (eulerY > 337.5f) ? Game.Directions.north : (Game.Directions)(int)MathF.Round(eulerY / 45);
    }
    public static int Euler(this Game.Directions direction) 
    {
        return (int)direction * 45;
    }
    public static Game.Directions Left(this Game.Directions direction, int amount = 2)
    {
        int directionInt = (int)direction - amount;
        return (directionInt < 0) ? (Game.Directions)(directionInt + 8) : (Game.Directions)directionInt;
    }
    public static Game.Directions Right(this Game.Directions direction, int amount = 2)
    {
        int directionInt = (int)direction + amount;
        return (directionInt > 7) ? (Game.Directions)(directionInt - 8) : (Game.Directions)directionInt;
    }
    public static Game.Directions Opposite(this Game.Directions direction)
    {
        int directionInt = (int)direction + 4;
        return (directionInt > 7) ? (Game.Directions)(directionInt - 8) : (Game.Directions)directionInt;
    }
    public static Vector3 VectorNormal(this Game.Directions direction, bool averageDiagonals = true)
    {
        return direction switch
        {
            Game.Directions.north => Vector3.forward,
            Game.Directions.northEast => (Vector3.forward + Vector3.right) * (averageDiagonals ? 0.75f : 1),
            Game.Directions.east => Vector3.right,
            Game.Directions.southEast => (Vector3.back + Vector3.right) * (averageDiagonals ? 0.75f : 1),
            Game.Directions.south => Vector3.back,
            Game.Directions.southWest => (Vector3.back + Vector3.left) * (averageDiagonals ? 0.75f : 1),
            Game.Directions.west => Vector3.left,
            Game.Directions.northWest => (Vector3.forward + Vector3.left) * (averageDiagonals ? 0.75f : 1),
            _ => Vector3.zero,
        };
    }
    public static Vector3Int VectorNormal(this Game.Directions direction)
    {
        return direction switch
        {
            Game.Directions.north => Vector3Int.forward,
            Game.Directions.northEast => Vector3Int.forward + Vector3Int.right,
            Game.Directions.east => Vector3Int.right,
            Game.Directions.southEast => Vector3Int.back + Vector3Int.right,
            Game.Directions.south => Vector3Int.back,
            Game.Directions.southWest => Vector3Int.back + Vector3Int.left,
            Game.Directions.west => Vector3Int.left,
            Game.Directions.northWest => Vector3Int.forward + Vector3Int.left,
            _ => Vector3Int.zero,
        };
    }
    public static Game.Directions VectorNormalToCardinal(this Vector3 direction, bool averageDiagonals = true)
    {
        if (direction == Vector3.forward) { return Game.Directions.north; }
        else if (direction == Vector3.forward + Vector3.right) { return Game.Directions.northEast; }
        else if (direction == Vector3.right) { return Game.Directions.east; }
        else if (direction == Vector3.back + Vector3.right) { return Game.Directions.southEast; }
        else if (direction == Vector3.back) { return Game.Directions.south; }
        else if (direction == Vector3.back + Vector3.left) { return Game.Directions.southWest; }
        else if (direction == Vector3.left) { return Game.Directions.west; }
        else if (direction == Vector3.forward + Vector3.left) { return Game.Directions.northWest; }
        else { return Game.Directions.north; };
    }
    public static Game.Directions VectorNormalToCardinal(this Vector3Int direction)
    {
        if (direction == Vector3Int.forward) { return Game.Directions.north; }
        else if (direction == Vector3Int.forward + Vector3Int.right) { return Game.Directions.northEast; }
        else if (direction == Vector3Int.right) { return Game.Directions.east; }
        else if (direction == Vector3Int.back + Vector3Int.right) { return Game.Directions.southEast; }
        else if (direction == Vector3Int.back) { return Game.Directions.south; }
        else if (direction == Vector3Int.back + Vector3Int.left) { return Game.Directions.southWest; }
        else if (direction == Vector3Int.left) { return Game.Directions.west; }
        else if (direction == Vector3Int.forward + Vector3Int.left) { return Game.Directions.northWest; }
        else { return Game.Directions.north; };
    }
    public static Vector3 Multiply(this Vector3 vector, Vector3 multiplyWith)
    {
        return new Vector3(vector.x * multiplyWith.x, vector.y * multiplyWith.y, vector.z * multiplyWith.z);
    }
    public static bool RandomBool()
    {
        return Convert.ToBoolean(Game.instance.random.Next(0, 2));
    }
    public static void IfFalseIgnore(this ref bool value, bool statement, bool output = true)
    {
        if (statement) { value = output; }
    }
    public static bool IfFalseIgnore(bool value, bool statement, bool output = true)
    {
        return statement ? output : value;
    }
    public static int Diff(int value1, int value2)
    {
        int 
            a = Math.Max(value1, value2),
            b = Math.Min(value1, value2);
        return a - b;
    }
    public static Vector3Int Diff(Vector3Int value1, Vector3Int value2)
    {
        return new Vector3Int(Diff(value1.x, value2.x), Diff(value1.y, value2.y), Diff(value1.z, value2.z));
    }
}