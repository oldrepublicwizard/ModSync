// Matching PyKotor implementation at vendor/PyKotor/Libraries/PyKotor/src/pykotor/tslpatcher/writer.py:128-143
// Original: @dataclass class ModificationsByType: ...
using System.Collections.Generic;
using HoloPatcher.Mods.GFF;
using HoloPatcher.Mods.NCS;
using HoloPatcher.Mods.NSS;
using HoloPatcher.Mods.SSF;
using HoloPatcher.Mods.TLK;
using HoloPatcher.Mods.TwoDA;

namespace HoloPatcher.Mods
{
    // Matching PyKotor implementation at vendor/PyKotor/Libraries/PyKotor/src/pykotor/tslpatcher/writer.py:128-143
    // Original: @dataclass class ModificationsByType: ...
    public class ModificationsByType
    {
        public List<ModificationsTLK> Tlk { get; set; } = new List<ModificationsTLK>();
        public List<InstallFile> Install { get; set; } = new List<InstallFile>();
        public List<Modifications2DA> Twoda { get; set; } = new List<Modifications2DA>();
        public List<ModificationsGFF> Gff { get; set; } = new List<ModificationsGFF>();
        public List<ModificationsSSF> Ssf { get; set; } = new List<ModificationsSSF>();
        public List<ModificationsNCS> Ncs { get; set; } = new List<ModificationsNCS>();
        public List<ModificationsNSS> Nss { get; set; } = new List<ModificationsNSS>();

        // Matching PyKotor implementation at vendor/PyKotor/Libraries/PyKotor/src/pykotor/tslpatcher/writer.py:140-143
        // Original: @classmethod def create_empty(cls) -> ModificationsByType: ...
        public static ModificationsByType CreateEmpty()
        {
            return new ModificationsByType
            {
                Twoda = new List<Modifications2DA>(),
                Gff = new List<ModificationsGFF>(),
                Tlk = new List<ModificationsTLK>(),
                Ssf = new List<ModificationsSSF>(),
                Ncs = new List<ModificationsNCS>(),
                Nss = new List<ModificationsNSS>(),
                Install = new List<InstallFile>()
            };
        }
    }
}

