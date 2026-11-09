using System.IO;
using UnityEngine;

public static class DataConverter
{
    public static T BytesToType<T>(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
            return (T)GlobalSerializer.Deserialize(stream);
    }

    public static byte[] TypeToBytes<T>(T value)
    {
        using (var stream = new MemoryStream())
        {
            GlobalSerializer.Serialize(stream, value);
            return stream.ToArray();
        }
    }

    public static byte[] AddFishEntityToBytes(string ownerName, string imageUrl, Vector3 position)
    {
        using (var stream = new MemoryStream())
        {
            using (var serializer = new BinarySerializer(stream))
            {
                serializer.Serialize(ownerName);
                serializer.Serialize(imageUrl);
                serializer.Serialize(position);
                return stream.ToArray();
            }
        }
    }

    public static (string, string, Vector3) BytesToAddFishEntity(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        {
            using (var deserializer = new BinaryDeserializer(stream))
            {
                var ownerName = (string)deserializer.Deserialize();
                var imageUrl = (string)deserializer.Deserialize();
                var position = (Vector3)deserializer.Deserialize();
                return (ownerName, imageUrl, position);
            }
        }
    }

    public static byte[] AddMagicEntityToBytes(string userName, Vector3 position)
    {
        using (var stream = new MemoryStream())
        {
            using (var serializer = new BinarySerializer(stream))
            {
                serializer.Serialize(userName);
                serializer.Serialize(position);
                return stream.ToArray();
            }
        }
    }

    public static (string, Vector3) BytesToAddMagicEntity(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        {
            using (var deserializer = new BinaryDeserializer(stream))
            {
                var userName = (string)deserializer.Deserialize();
                var position = (Vector3)deserializer.Deserialize();
                return (userName, position);
            }
        }
    }
}