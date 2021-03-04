# BitStream
A bitstream that offers (de)serialization in binary format

BitStream is written in NetStandard 2.0 making it compatible with Unity3D.

A serializer/stream for reading/writing various types of data. Similar to https://github.com/DennisCorvers/ByteStream but instead packs on bits instead of bytes. This means that with BitStream, the smallest possible read and write is 1 bit long. This allows for compressing data (like bit flags) down by up to 8-fold!

*__Carefully read the Usage section__ for a brief introduction on how to use the library.*

## What does Bitsteam offer?
Bitstream consists of one class that offers a series of write and read operations for various data types. It writes internally to memory which is either automatically allocated or supplied by the user.

- Writes bits instead of bytes severely reducing the amount of data required.
- Allows for minimum and maximum numeric values for further reducing data size.
- Allows for various string formats to be written and read.
- Allows for float precision to even further reduce data size.
- Automatically keeps track of offsets and buffer boundaries.
- Prefixes strings and memory blocks with a 2-byte length.
- Works with all blittable types
- Fluent pattern for easy chaining of operations

## Supported types:
- All types that adhere to the [unmanaged constraint](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types) can be written to, and read from the buffer.
- Byte arrays
- Various string formats


# Technical specifications:

The benchmark tests the writing of 1 boolean value followed by 1 integer value (semi-random) for 1024 times. After writing, the same process is repeated for reading.

A boolean value is written and read to ensure a worst-case scenario for the BitStream (due to misaligned offsets).


|       Method |     Mean |     Error |    StdDev | Ratio |
|------------- |---------:|----------:|----------:|------:|
|    ByteWrite | 3.743 μs | 0.0196 μs | 0.0183 μs |  1.02 |
|     PtrWrite | 2.940 μs | 0.0041 μs | 0.0038 μs |  0.80 |
|    BitStream | 3.939 μs | 0.0455 μs | 0.0404 μs |  1.07 |
| ArraySegment | 3.681 μs | 0.0082 μs | 0.0069 μs |  1.00 |
|   BitConvert | 6.988 μs | 0.0308 μs | 0.0273 μs |  1.90 |

**Why should you use BitStream?**

BitStream can severely reduce the amount of data used when serializing data.

Imagine serializing the following object, where the mana can range from 0 to 100, and the hp from 0 to 999. Normal serialization would result in a size of 27 bytes. 

When using BitStream, we can choose to limit the precision of the Vectors. This results in a byte size of 15! That's a difference of almost 45%

# Usage

It is important that you write and read back the values in the same order to keep data consistent (as shown in the write and read example).

**BitStream is not inherently thread safe! Beware of accessing buffers concurrently from multiple threads and modifying the supplied buffers during operation!**

### Serializing types

The following example shows how the BitStream can be used to serialize a type. Depending on the mode the Stream is in (Read or Write), the Player class is either restored from the Stream, or written to the Stream.
```C#
    public class Player
    {
        Vector3 position;
        Vector3 rotation;

        byte mana = 100;
        ushort hp = 100;

        public void BitSerialize(BitStreamer stream)
        {
            stream.Serialize(ref position, true);
            stream.Serialize(ref rotation, true);
            stream.Serialize(ref mana, 0, 100);
            stream.Serialize(ref hp, 0, 999);
        }
    }
```

### Important notes!

Be careful when calling ResetRead and ResetWrite methods! Make sure you are not setting or copying a new buffer over the old one. The BitStream can be disposed and immediately used again.

### Basic usage (allowing the BitStream to create an internal buffer)

```C#
// Creating new stream. ResetWrite resets and allocates memory for writing.
BitStream stream = new BitStream();
stream.ResetWrite(64);

// Write various data types to the stream.
stream.WriteInt32(135, 0, 999);

float speed = 3.5f;
stream.Serialize(ref speed);

Vector2 size = new Vector2(4, 3);
stream.WriteBlit(size);


// Reset the stream for reading operations
stream.ResetRead();

// Read back the data
int hp = stream.ReadInt32(0, 999);
stream.Serialize(ref speed);
size = stream.ReadBlit<Vector2>();


// Don't forget to dispose after use!
stream.Dispose();

//Chaining of write operations
stream.WriteInt32(playerID)
      .WriteBool(player.IsAlive)
      .WriteUShort(playerHP, 0, 10000);

```
