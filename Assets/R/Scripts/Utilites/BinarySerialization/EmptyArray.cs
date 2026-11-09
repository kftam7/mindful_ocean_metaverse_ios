// Copyright 2022 Niantic, Inc. All Rights Reserved.

/// <summary>
/// Generic class that generates a single, reusable instance of an empty array of type T.
/// </summary>
public static class EmptyArray<T>
{
    public static readonly T[] Instance = new T[0];
}