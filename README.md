# BitStream
A bitstream that offers (de)serialization in binary format

A serializer/stream for reading/writing various types of data. Similar to https://github.com/DennisCorvers/ByteStream but instead packs on bits instead of bytes. This means that with BitStream, the smallest possible read and write is 1 bit long. This allows for compressing data (like bit flags) down by up to 8-fold!

*__Carefully read the Usage section__ for a brief introduction on how to use the library.*

## What does Bitsteam offer?
Bitstream consists of one class that offers a series of write and read operations for various data types. It writes internally to memory which is either automatically allocated or supplied by the user.

- Writes bits instead of bytes severely reducing the amount of data required.
- Allows for minimum and maximum numeric values for further reducing data size.
- Allows for various string formats to be written and read.
- Allows for float precision to even further reduce data size.
- Bitstream is a value type which means it can be created at will without generating garbage.
- Automatically keeps track of offsets and buffer boundaries.
- Prefixes strings and memory blocks with a 2-byte length.
- Works with all blittable types

## Supported types:
- All types that adhere to the [unmanaged constraint](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types) can be written to, and read from the buffer.
- Byte arrays
- Various string formats


# Technical specifications:
Benchmarks yet to come...


# Usage

Because the API is almost identical, the managed and unmanaged readers and writers function nearly identical. Be warned that when using the unmanaged reader or writer, the provided length must be no longer than the actual length of the memory block provided.

It is important that you write and read back the values in the same order to keep data consistent (as shown in the write and read example).

**BitStream is not inherently thread safe! Beware of accessing buffers concurrently from multiple threads and modifying the supplied buffers during operation!**

### Important notes!

Be careful when calling ResetRead and ResetWrite methods! Make sure you are not setting or copying a new buffer over the old one. The BitStream can be disposed and immediately used again. **Always dispose BitStream after usage and before setting a new buffer!**

### Basic usage (allowing the BitStream to create an internal buffer)

```C#
//Creating new stream. ResetWrite resets and allocates memory for writing.
BitStream stream = new BitStream();
stream.ResetWrite(64);

//Write various data types to the stream.
stream.WriteInt32(135, 0, 999);

float speed = 3.5f;
stream.Serialize(ref speed);

Vector2 size = new Vector2(4, 3);
stream.WriteBlit(size);

//Reset the stream for reading operations if its's in write!
stream.ResetRead();

//Read back the data...
int hp = stream.ReadInt32(0, 999);
stream.Serialize(ref speed);
size = stream.ReadBlit<Vector2>();

//IMPORTANT Don't forget to dispose after use!
stream.Dispose();

```
