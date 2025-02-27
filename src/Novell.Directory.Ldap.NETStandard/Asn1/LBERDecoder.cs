﻿/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     This class provides LBER decoding routines for ASN.1 Types. LBER is a
    ///     subset of BER as described in the following taken from 5.1 of RFC 2251:
    ///     5.1. Mapping Onto BER-based Transport Services
    ///     The protocol elements of Ldap are encoded for exchange using the
    ///     Basic Encoding Rules (BER) [11] of ASN.1 [3]. However, due to the
    ///     high overhead involved in using certain elements of the BER, the
    ///     following additional restrictions are placed on BER-encodings of Ldap
    ///     protocol elements:
    ///     <li>(1) Only the definite form of length encoding will be used.</li>
    ///     <li>(2) OCTET STRING values will be encoded in the primitive form only.</li>
    ///     <li>
    ///         (3) If the value of a BOOLEAN type is true, the encoding MUST have
    ///         its contents octets set to hex "FF".
    ///     </li>
    ///     <li>
    ///         (4) If a value of a type is its default value, it MUST be absent.
    ///         Only some BOOLEAN and INTEGER types have default values in this
    ///         protocol definition.
    ///         These restrictions do not apply to ASN.1 types encapsulated inside of
    ///         OCTET STRING values, such as attribute values, unless otherwise
    ///         noted.
    ///     </li>
    ///     [3] ITU-T Rec. X.680, "Abstract Syntax Notation One (ASN.1) -
    ///     Specification of Basic Notation", 1994.
    ///     [11] ITU-T Rec. X.690, "Specification of ASN.1 encoding rules: Basic,
    ///     Canonical, and Distinguished Encoding Rules", 1994.
    /// </summary>
    public class LberDecoder : IAsn1Decoder
    {
        // used to speed up decode, so it doesn't need to recreate an identifier every time
        // instead just reset is called CANNOT be static for multiple connections
        private readonly Asn1Identifier _asn1Id = new Asn1Identifier();
        private readonly Asn1Length _asn1Len = new Asn1Length();

        /* Generic decode routines
        */

        /// <summary> Decode an LBER encoded value into an Asn1Type from a byte array.</summary>
        public Asn1Object Decode(byte[] valueRenamed)
        {
            Asn1Object asn1 = null;

            var inRenamed = new MemoryStream(valueRenamed);
            try
            {
                asn1 = Decode(inRenamed, default).GetAwaiter().GetResult();
            }
            catch (IOException ioe)
            {
                Logger.Log.LogWarning("Exception swallowed", ioe);
            }

            return asn1;
        }

        /// <summary> Decode an LBER encoded value into an Asn1Type from an InputStream.</summary>
        public ValueTask<Asn1Object> Decode(Stream inRenamed, CancellationToken cancellationToken)
        {
            var len = new int[1];
            return Decode(inRenamed, len, cancellationToken);
        }

        /// <summary>
        ///     Decode an LBER encoded value into an Asn1Object from an InputStream.
        ///     This method also returns the total length of this encoded
        ///     Asn1Object (length of type + length of length + length of content)
        ///     in the parameter len. This information is helpful when decoding
        ///     structured types.
        /// </summary>
        public async ValueTask<Asn1Object> Decode(Stream inRenamed, int[] len, CancellationToken cancellationToken)
        {
            _asn1Id.Reset(inRenamed);
            _asn1Len.Reset(inRenamed);

            var length = _asn1Len.Length;
            len[0] = _asn1Id.EncodedLength + _asn1Len.EncodedLength + length;

            if (_asn1Id.IsUniversal)
            {
                switch (_asn1Id.Tag)
                {
                    case Asn1Sequence.Tag:
                        return new Asn1Sequence(await Asn1Structured.DecodeStructured(this, inRenamed, length, cancellationToken).ConfigureAwait(false));

                    case Asn1Set.Tag:
                        return new Asn1Set(await Asn1Structured.DecodeStructured(this, inRenamed, length, cancellationToken).ConfigureAwait(false));

                    case Asn1Boolean.Tag:
                        return new Asn1Boolean(await DecodeBoolean(inRenamed, length, cancellationToken).ConfigureAwait(false));

                    case Asn1Integer.Tag:
                        return new Asn1Integer(await DecodeNumeric(inRenamed, length, cancellationToken).ConfigureAwait(false));
                    case Asn1OctetString.Tag:
                        return new Asn1OctetString(length > 0 ? await DecodeOctetString(inRenamed, length, cancellationToken).ConfigureAwait(false) : new byte[0]);

                    case Asn1Enumerated.Tag:
                        return new Asn1Enumerated(await DecodeNumeric(inRenamed, length, cancellationToken).ConfigureAwait(false));

                    case Asn1Null.Tag:
                        return new Asn1Null(); // has no content to decode.
                    /* Asn1 TYPE NOT YET SUPPORTED
                    case Asn1BitString.TAG:
                    return new Asn1BitString(this, in, length);
                    case Asn1ObjectIdentifier.TAG:
                    return new Asn1ObjectIdentifier(this, in, length);
                    case Asn1Real.TAG:
                    return new Asn1Real(this, in, length);
                    case Asn1NumericString.TAG:
                    return new Asn1NumericString(this, in, length);
                    case Asn1PrintableString.TAG:
                    return new Asn1PrintableString(this, in, length);
                    case Asn1TeletexString.TAG:
                    return new Asn1TeletexString(this, in, length);
                    case Asn1VideotexString.TAG:
                    return new Asn1VideotexString(this, in, length);
                    case Asn1IA5String.TAG:
                    return new Asn1IA5String(this, in, length);
                    case Asn1GraphicString.TAG:
                    return new Asn1GraphicString(this, in, length);
                    case Asn1VisibleString.TAG:
                    return new Asn1VisibleString(this, in, length);
                    case Asn1GeneralString.TAG:
                    return new Asn1GeneralString(this, in, length);
                    */

                    default:
                        throw new EndOfStreamException("Unknown tag"); // !!! need a better exception
                }
            }

            // APPLICATION or CONTEXT-SPECIFIC tag
            return new Asn1Tagged((Asn1Identifier)_asn1Id.Clone(), new Asn1OctetString(length > 0 ? await DecodeOctetString(inRenamed, length, cancellationToken).ConfigureAwait(false) : new byte[0]));
        }

        /* Decoders for ASN.1 simple type Contents
        */

        /// <summary> Decode a boolean directly from a stream.</summary>
        public async ValueTask<bool> DecodeBoolean(Stream inRenamed, int len, CancellationToken cancellationToken)
        {
            var lber = new byte[len];

            var i = await ReadInput(inRenamed, lber, 0, lber.Length, cancellationToken).ConfigureAwait(false);

            if (i != len)
            {
                throw new EndOfStreamException("LBER: BOOLEAN: decode error: EOF");
            }

            return lber[0] != 0x00;
        }

        /// <summary>
        ///     Decode a Numeric type directly from a stream. Decodes INTEGER
        ///     and ENUMERATED types.
        /// </summary>
        public async ValueTask<long> DecodeNumeric(Stream inRenamed, int len, CancellationToken cancellationToken)
        {
            long l = 0;
            var r = (long)inRenamed.ReadByte();

            if (r < 0)
            {
                throw new EndOfStreamException("LBER: NUMERIC: decode error: EOF");
            }

            if ((r & 0x80) != 0)
            {
                // check for negative number
                l = -1;
            }

            l = (l << 8) | r;

            var octets = new byte[len - 1];
#if NETSTANDARD2_0
            if (len - 1 > await inRenamed.ReadAsync(octets, 0, octets.Length, cancellationToken).ConfigureAwait(false))
#else
            if (len - 1 > await inRenamed.ReadAsync(octets.AsMemory(), cancellationToken).ConfigureAwait(false))
#endif
            {
                throw new EndOfStreamException("LBER: CHARACTER STRING: decode error: EOF");
            }

            for (var i = 0; i < len - 1; i++)
            {
                l = (l << 8) | octets[i];
            }

            return l;
        }

        /// <summary> Decode an OctetString directly from a stream.</summary>
        public async ValueTask<byte[]> DecodeOctetString(Stream inRenamed, int len, CancellationToken cancellationToken)
        {
            var octets = new byte[len];
            var totalLen = 0;

            while (totalLen < len)
            {
                // Make sure we have read all the data
                var inLen = await ReadInput(inRenamed, octets, totalLen, len - totalLen, cancellationToken).ConfigureAwait(false);
                totalLen += inLen;
            }

            return octets;
        }

        /// <summary> Decode a CharacterString directly from a stream.</summary>
        public async ValueTask<string> DecodeCharacterString(Stream inRenamed, int len, CancellationToken cancellationToken)
        {
            var octets = new byte[len];
#if NETSTANDARD2_0
            if (len > await inRenamed.ReadAsync(octets, 0, len, cancellationToken).ConfigureAwait(false))
#else
            if (len > await inRenamed.ReadAsync(octets.AsMemory(), cancellationToken).ConfigureAwait(false))
#endif
            {
                throw new EndOfStreamException("LBER: CHARACTER STRING: decode error: EOF");
            }

            var rval = octets.ToUtf8String();

            return rval; // new String( "UTF8");
        }

        /// <summary>
        ///     Reads a number of characters from the current source Stream and writes the data to the target array at the
        ///     specified index.
        /// </summary>
        /// <param name="sourceStream">The source Stream to read from.</param>
        /// <param name="target">Contains the array of characteres read from the source Stream.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source Stream.</param>
        /// <returns>
        ///     The number of characters read. The number will be less than or equal to count depending on the data available
        ///     in the source Stream. Returns -1 if the end of the stream is reached.
        /// </returns>
        private static async ValueTask<int> ReadInput(Stream sourceStream, byte[] target, int start, int count, CancellationToken cancellationToken)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
            {
                return 0;
            }
#if NETSTANDARD2_0
            var bytesRead = await sourceStream.ReadAsync(target, start, count, cancellationToken).ConfigureAwait(false);
#else
            var bytesRead = await sourceStream.ReadAsync(target.AsMemory(start, count), cancellationToken).ConfigureAwait(false);
#endif

            // Returns -1 if EOF
            if (bytesRead <= 0)
            {
                return -1;
            }

            return bytesRead;
        }
    }
}
