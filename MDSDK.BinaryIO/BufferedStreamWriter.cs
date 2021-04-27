// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;

namespace MDSDK.BinaryIO
{
    public sealed class BufferedStreamWriter
    {
        public Stream Stream { get; }

        private readonly byte[] _buffer;

        private int _bufferWritePointer;

        private const int BufferSize = 4096;

        private const int MaxWriteToBufferByteCount = 1024;

        private long _position;

        public BufferedStreamWriter(Stream stream)
        {
            Stream = stream;

            _buffer = new byte[BufferSize];
            _bufferWritePointer = 0;

            _position = stream.CanSeek ? stream.Position : 0;
        }

        public long Position
        {
            get { return _position; }
            internal set { _position = value; }
        }

        private void BeginWriteToBuffer(int byteCount)
        {
            Debug.Assert(byteCount <= MaxWriteToBufferByteCount);

            if (_bufferWritePointer + byteCount > BufferSize)
            {
                Flush(FlushMode.Shallow);
            }
        }

        private void EndWriteToBuffer(int byteCount)
        {
            _bufferWritePointer += byteCount;
        }

        public void WriteByte(byte datum)
        {
            BeginWriteToBuffer(1);
            _buffer[_bufferWritePointer] = datum;
            EndWriteToBuffer(1);
            Position++;
        }

        internal Span<byte> GetBufferWriteSpan(int count)
        {
            BeginWriteToBuffer(count);
            var bufferWriteSpan = _buffer.AsSpan(_bufferWritePointer, count);
            EndWriteToBuffer(count);
            return bufferWriteSpan;
        }
        
        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            if (data.Length <= MaxWriteToBufferByteCount)
            {
                var bufferWriteSpan = GetBufferWriteSpan(data.Length);
                data.CopyTo(bufferWriteSpan);
            }
            else
            {
                Flush(FlushMode.Shallow);
                Stream.Write(data);
            }

            Position += data.Length;
        }

        public void WriteZeros(int count)
        {
            for (var i = 0; i < count; i++)
            {
                WriteByte(0);
            }
        }

        public void Flush(FlushMode mode)
        {
            if (_bufferWritePointer > 0)
            {
                Stream.Write(_buffer.AsSpan(0, _bufferWritePointer));
                _bufferWritePointer = 0;

                if (mode == FlushMode.Deep)
                {
                    Stream.Flush();
                }
            }
        }
    }
}
