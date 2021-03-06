﻿/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.RedFaction3.FileFormats
{
    public class AsmFile
    {
        private Endian _Endian;
        private ushort _Version;
        private readonly List<Asm.ContainerEntry> _Containers;

        public AsmFile()
        {
            this._Containers = new List<Asm.ContainerEntry>();
        }

        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public ushort Version
        {
            get { return this._Version; }
            set { this._Version = value; }
        }

        public List<Asm.ContainerEntry> Containers
        {
            get { return this._Containers; }
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32();
            if (magic != 0xBEEFFEED)
            {
                throw new FormatException("not an asm file");
            }

            var endian = magic == 0xBEEFFEED ? Endian.Little : Endian.Big;

            var version = input.ReadValueU16(endian);
            if (version != 5)
            {
                throw new FormatException("unsupported asm version " + this._Version.ToString());
            }

            var containerCount = input.ReadValueU16();

            var containers = new Asm.ContainerEntry[containerCount];
            for (ushort i = 0; i < containerCount; i++)
            {
                var container = new Asm.ContainerEntry();
                container.Deserialize(input, this._Version, endian);
                containers[i] = container;
            }

            this._Endian = endian;
            this._Version = version;
            this._Containers.Clear();
            this._Containers.AddRange(containers);
        }

        public void Serialize(Stream output)
        {
            var endian = this._Endian;

            if (this._Containers.Count > ushort.MaxValue)
            {
                throw new FormatException("too many containers");
            }
            var containerCount = (ushort)this._Containers.Count;

            output.WriteValueU32(0xBEEFFEED, endian);
            output.WriteValueU16(this._Version, endian);
            output.WriteValueU16(containerCount, endian);
            for (ushort i = 0; i < containerCount; i++)
            {
                this._Containers[i].Serialize(output, this._Version, endian);
            }
        }
    }
}
