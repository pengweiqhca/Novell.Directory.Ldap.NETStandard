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

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents and Ldap Bind Request.
    ///     <pre>
    ///         BindRequest ::= [APPLICATION 0] SEQUENCE {
    ///         version                 INTEGER (1 .. 127),
    ///         name                    LdapDN,
    ///         authentication          AuthenticationChoice }
    ///     </pre>
    /// </summary>
    public class RfcBindRequest : Asn1Sequence, IRfcRequest
    {
        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier Id = new Asn1Identifier(Asn1Identifier.Application, true,
            LdapMessage.BindRequest);

        // *************************************************************************
        // Constructors for BindRequest
        // *************************************************************************

        /// <summary> </summary>
        public RfcBindRequest(Asn1Integer version, RfcLdapDn name, RfcAuthenticationChoice auth)
            : base(3)
        {
            Add(version);
            Add(name);
            Add(auth);
        }

        public RfcBindRequest(int version, string dn, string mechanism, byte[] credentials)
            : this(new Asn1Integer(version), new RfcLdapDn(dn), new RfcAuthenticationChoice(mechanism, credentials))
        {
        }

        /// <summary>
        ///     Constructs a new Bind Request copying the original data from
        ///     an existing request.
        /// </summary>
        internal RfcBindRequest(Asn1Object[] origRequest, string baseRenamed)
            : base(origRequest)
        {
            // Replace the dn if specified, otherwise keep original base
            if (baseRenamed != null)
            {
                set_Renamed(1, new RfcLdapDn(baseRenamed));
            }
        }

        /// <summary> </summary>
        /// <summary> Sets the protocol version.</summary>
        public Asn1Integer Version
        {
            get => (Asn1Integer)get_Renamed(0);

            set => set_Renamed(0, value);
        }

        /// <summary> </summary>
        /// <summary> </summary>
        public RfcLdapDn Name
        {
            get => (RfcLdapDn)get_Renamed(1);

            set => set_Renamed(1, value);
        }

        /// <summary> </summary>
        /// <summary> </summary>
        public RfcAuthenticationChoice AuthenticationChoice
        {
            get => (RfcAuthenticationChoice)get_Renamed(2);

            set => set_Renamed(2, value);
        }

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool request)
        {
            return new RfcBindRequest(ToArray(), baseRenamed);
        }

        public string GetRequestDn()
        {
            return ((RfcLdapDn)get_Renamed(1)).StringValue();
        }

        // *************************************************************************
        // Mutators
        // *************************************************************************

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 0. (0x60)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return Id;
        }
    }
}
