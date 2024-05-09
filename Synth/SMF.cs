using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Synth {
	public partial class SMF {
		/// <summary>
		/// イベントリスト
		/// </summary>
		public List<Event> EventList = new List<Event>();

		int mTrackCount;
		int mUnitTick;

		/// <summary>
		/// ファイルを開く
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		public SMF(string filePath) {
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) {
				return;
			}
			using (var fs = new FileStream(filePath, FileMode.Open)) {
				var br = new BinaryReader(fs);
				LoadMThd(br);
				for (int i = 0; i < mTrackCount; i++) {
					LoadMTrk(br, i);
				}
			}
			EventList.RemoveAll(x => x.MetaType == META.EOT);
			EventList.Sort((a, b) => {
				var diffTick = a.Tick - b.Tick;
				return (0 == diffTick) ? (a.Track - b.Track) : diffTick;
			});
		}

		/// <summary>
		/// ファイルに保存
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		public void Save(string filePath) {
			var trackList = new List<int>();
			foreach (var ev in EventList) {
				if (!trackList.Contains(ev.Track)) {
					trackList.Add(ev.Track);
				}
			}
			trackList.Sort();
			using (var fs = new FileStream(filePath, FileMode.Create)) {
				SaveMThd(fs, trackList.Count);
				foreach (var trackNum in trackList) {
					SaveMTrk(fs, trackNum);
				}
			}
		}

		void LoadMThd(BinaryReader br) {
			var signature = Encoding.ASCII.GetString(br.ReadBytes(4));
			var size = ReadUInt32(br);
			if ("MThd" != signature || 6 != size) {
				throw new Exception();
			}
			var format = ReadUInt16(br);
			if (format >= 3) {
				throw new Exception();
			}
			mTrackCount = ReadUInt16(br);
			mUnitTick = ReadUInt16(br);
		}

		void LoadMTrk(BinaryReader br, int number) {
			var signature = Encoding.ASCII.GetString(br.ReadBytes(4));
			if ("MTrk" != signature) {
				EventList.Clear();
				throw new Exception();
			}
			var size = ReadUInt32(br);
			var end = br.BaseStream.Position + size;
			var tick = 0;
			byte lastStatus = 0;
			while (br.BaseStream.Position < end) {
				tick += ReadDelta(br);
				var data = ReadMessage(br, ref lastStatus);
				var type = (MSG)(data[0] < (int)MSG.SYS_EX ? (data[0] & 0xF0) :  data[0]);
				Event ev;
				switch (type) {
				case MSG.NOTE_OFF:
				case MSG.NOTE_ON:
					ev = new Note(data);
					break;
				case MSG.KEY_PRESS:
					ev = new Event(data);
					break;
				case MSG.CTRL_CHG:
					ev = new Ctrl(data);
					break;
				case MSG.PROG_CHG:
					ev = new Prog(data);
					break;
				case MSG.CH_PRESS:
					ev = new Event(data);
					break;
				case MSG.PITCH:
					ev = new Pitch(data);
					break;
				case MSG.SYS_EX:
					ev = new Event(data);
					break;
				case MSG.META:
					switch ((META)data[1]) {
					case META.TEMPO:
						ev = new Tempo(data);
						break;
					case META.MEASURE:
						ev = new Measure(data);
						break;
					case META.KEY:
						ev = new Key(data);
						break;
					case META.DATA:
						ev = new Binary(data);
						break;
					default:
						if (Enum.IsDefined(typeof(TEXT), (META)data[1])) {
							ev = new Text(data);
						} else {
							ev = new Event(data);
						}
						break;
					}
					break;
				default:
					throw new Exception();
				}
				ev.Tick = tick * UnitTick / mUnitTick;
				ev.Track = number;
				EventList.Add(ev);
			}
		}

		void SaveMThd(FileStream fs, int trackCount) {
			Write(fs, (uint)0x4D546864);
			Write(fs, (uint)6);
			Write(fs, (ushort)1);
			Write(fs, (ushort)trackCount);
			Write(fs, (ushort)UnitTick);
		}

		void SaveMTrk(FileStream fs, int trackNum) {
			var eventList = EventList.FindAll(x => x.Track == trackNum);
			eventList.Sort((a, b) => a.Tick - b.Tick);
			// EOTをトラックの終端に追加
			{
				var lastEvent = eventList[eventList.Count - 1];
				var eot = new Event((byte)MSG.META, (byte)META.EOT, 0) {
					Tick = lastEvent.Tick
				};
				eventList.Add(eot);
			}
			// トラックのバイト数をカウント
			int trackSize = 0;
			{
				int lastTick = 0;
				foreach (var ev in eventList) {
					trackSize += GetDelta(ev.Tick - lastTick).Length + ev.Msg.Length;
					lastTick = ev.Tick;
				}
			}
			// MTrkを書き込み
			{
				Write(fs, (uint)0x4D54726B);
				Write(fs, (uint)trackSize);
				var bw = new BinaryWriter(fs);
				var lastTick = 0;
				foreach (var ev in eventList) {
					bw.Write(GetDelta(ev.Tick - lastTick));
					bw.Write(ev.Msg);
					lastTick = ev.Tick;
				}
			}
		}

		static byte[] ReadMessage(BinaryReader br, ref byte lastStatus) {
			var status = br.ReadByte();
			byte data1;
			if (status < 0x80) {
				data1 = status;
				status = lastStatus;
			} else {
				data1 = br.ReadByte();
				lastStatus = status;
			}

			var msgType = (MSG)(status < (int)MSG.SYS_EX ? (status & 0xF0) : status);
			switch (msgType) {
			case MSG.PROG_CHG:
			case MSG.CH_PRESS:
				return new byte[] { status, data1 };
			case MSG.NOTE_OFF:
			case MSG.NOTE_ON:
			case MSG.KEY_PRESS:
			case MSG.CTRL_CHG:
			case MSG.PITCH:
				return new byte[] { status, data1, br.ReadByte() };
			case MSG.SYS_EX: {
				var arr = new List<byte>() { status, data1 };
				byte readByte;
				do {
					readByte = br.ReadByte();
					arr.Add(readByte);
				} while (readByte != 0xF7);
				return arr.ToArray();
			}
			case MSG.META: {
				var len = ReadDelta(br);
				var data = br.ReadBytes(len);
				var delta = GetDelta(len);
				var start = 2 + delta.Length;
				var ret = new byte[start + data.Length];
				ret[0] = status;
				ret[1] = data1;
				Array.Copy(delta, 0, ret, 2, delta.Length);
				Array.Copy(data, 0, ret, start, data.Length);
				return ret;
			}
			default:
				throw new Exception();
			}
		}

		static uint ReadUInt32(BinaryReader br) {
			var value = (uint)br.ReadByte() << 24;
			value |= (uint)br.ReadByte() << 16;
			value |= (uint)br.ReadByte() << 8;
			value |= br.ReadByte();
			return value;
		}

		static ushort ReadUInt16(BinaryReader br) {
			var value = (ushort)(br.ReadByte() << 8);
			value |= br.ReadByte();
			return value;
		}

		static int ReadDelta(BinaryReader br) {
			int value = 0;
			int readByte;
			do {
				value <<= 7;
				readByte = br.ReadByte();
				value |= readByte & 0x7F;
			} while (readByte >= 0x80);
			return value;
		}

		static void Write(FileStream fs, uint value) {
			fs.WriteByte((byte)(value >> 24));
			fs.WriteByte((byte)((value >> 16) & 0xFF));
			fs.WriteByte((byte)((value >> 8) & 0xFF));
			fs.WriteByte((byte)(value & 0xFF));
		}

		static void Write(FileStream fs, ushort value) {
			fs.WriteByte((byte)(value >> 8));
			fs.WriteByte((byte)(value & 0xFF));
		}

		public static byte[] GetDelta(int value) {
			if (value < (1 << 7)) {
				return new byte[] { (byte)value };
			} else if (value < (1 << 14)) {
				return new byte[] {
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			} else if (value < (1 << 21)) {
				return new byte[] {
					(byte)(0x80 | (value >> 14) & 0x7F),
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			} else {
				return new byte[] {
					(byte)(0x80 | (value >> 21) & 0x7F),
					(byte)(0x80 | (value >> 14) & 0x7F),
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			}
		}

		public static int GetDeltaSize(int start, params byte[] arr) {
			var index = start;
			var count = 0;
			while (count++ < 4 && arr[index] >= 0x80 && ++index < arr.Length) ;
			return count;
		}
	}
}
