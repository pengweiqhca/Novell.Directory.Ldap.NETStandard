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

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     The Asn1Set class can hold an unordered collection of components with
    ///     distinct type. This class inherits from the Asn1Structured class
    ///     which already provides functionality to hold multiple Asn1 components.
    /// </summary>
    public class Asn1Set : Asn1Structured
    {
        /// <summary> ASN.1 SET tag definition.</summary>
        public const int Tag = 0x11;

        /// <summary> ID is added for Optimization.</summary>
        /// <summary>
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1Identifier.Universal, true, Tag);

        /* Constructors for Asn1Set
                */

        /// <summary>
        ///     Constructs an Asn1Set object with no actual
        ///     Asn1Objects in it. Assumes a default size of 5 elements.
        /// </summary>
        public Asn1Set()
            : base(Id)
        {
        }

        /// <summary>
        ///     Constructs an Asn1Set object with the specified
        ///     number of placeholders for Asn1Objects. However there
        ///     are no actual Asn1Objects in this SequenceOf object.
        /// </summary>
        /// <param name="newContent">
        ///     the array containing the Asn1 data for the sequence.
        /// </param>
        public Asn1Set(Asn1Object[] newContent)
            : base(Id, newContent)
        {
        }

        /* Asn1Set specific methods
        */

        /// <summary> Returns a String representation of this Asn1Set.</summary>
        public override string ToString()
        {
            return ToString("SET: { ");
        }
    }
}
