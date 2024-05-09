using System.Collections.Generic;
using System.IO;

namespace Synth.Instruments {
	internal class Layer {
		internal CK_LAYER Header;
		internal List<CK_ART> Arts = new List<CK_ART>();
		internal void Load(FileStream fs, ref int artIndex) { }
		internal void Save(FileStream fs, ref int artCount) { }
	}
}
