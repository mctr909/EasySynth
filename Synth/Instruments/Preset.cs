using System.Collections.Generic;
using System.IO;

namespace Synth.Instruments {
	internal class Preset {
		internal CK_PRESET Header;
		internal List<Layer> Layers = new List<Layer>();
		internal void Load(FileStream fs, ref int layerIndex) { }
		internal void Save(FileStream fs, ref int layerCount) { }
	}
}
