namespace WidgetDepot.ApiService.Shared;

/// <summary>
/// Represents a type that has only one value, similar to 'void' in contexts where a type is required.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    // The single instance of Unit
    public static readonly Unit Value = new();

    // Equality members
    public override bool Equals(object? obj) => obj is Unit;
    public bool Equals(Unit other) => true;
    public override int GetHashCode() => 0;

    public static bool operator ==(Unit _, Unit __) => true;
    public static bool operator !=(Unit _, Unit __) => false;

    public override string ToString() => "()";
}