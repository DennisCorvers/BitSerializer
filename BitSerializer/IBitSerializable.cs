namespace BitSerializer
{
    public interface IBitSerializable
    {
        void BitSerialize(BitStreamer stream);
    }
}
