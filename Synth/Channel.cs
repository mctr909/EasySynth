using System;
using Synth.Instruments;

namespace Synth {
	public class Channel {
		const double THRESHOLD_STANDBY = 1e-8; //-80db
		const double THRESHOLD_ACTIVE = 1e-6; //-60db
		const double RMS_SPEED = 132.0;
		const double PEAK_ATT_DELTA = 2.0;
		const double PI2_SQRT3 = 3.6275987;

		public enum STATE : byte {
			STANDBY,
			STARTUP,
			ACTIVE
		}

		public readonly int Num;

		#region public properties
		public string Name { get; set; }

		public STATE State { get; private set; }

		public bool Mute { get; private set; }

		public bool IsDrum { get; private set; }
		public byte BankMsb { get; private set; }
		public byte BankLsb { get; private set; }
		public byte ProgNum { get; private set; }

		public bool Hold { get; private set; }

		public double Pitch { get; private set; }

		public double Vol { get; private set; }
		public double Exp { get; private set; }
		public double Pan { get; private set; }

		public double VibRange { get; private set; }
		public double VibDepth { get; private set; }
		public double VibRate { get; private set; }
		public double VibDelay { get; private set; }

		public double ChoSend { get; private set; }
		public double ChoRate { get; private set; }
		public double ChoDepth { get; private set; }

		public double DelSend { get; private set; }
		public double DelTime { get; private set; }
		public double DelFeedback { get; private set; }
		public double DelCross { get; private set; }

		public double Cutoff { get; private set; }
		public double Resonance { get; private set; }

		public double RmsL { get; private set; }
		public double RmsR { get; private set; }
		public double PeakL { get; private set; }
		public double PeakR { get; private set; }
		public bool EnableSpectrum {
			get { return mSpectrumGain > 0; }
			set { mSpectrumGain = value ? 1 : 0; }
		}
		#endregion

		#region internal properties
		internal double DeltaTime { get; private set; }
		internal double PanRe { get; private set; }
		internal double PanIm { get; private set; }
		internal int BufferSamples { get; private set; }
		internal double[] InputBufferL;
		internal double[] InputBufferR;
		internal WAVE_INFO[] WaveInfoTable { get; private set; }

		internal int RegionCount { get; private set; }
		internal REGION[] Regions { get; private set; }
		#endregion

		#region private variables
		int mChoIdx;
		int mChoSamples;
		double mChoLfoU;
		double mChoLfoV;
		double mChoLfoW;
		double[] mChoBufferL;
		double[] mChoBufferR;

		int mDelIdx;
		int mDelSamples;
		double[] mDelBufferL;
		double[] mDelBufferR;

		double mGain;
		double mSpectrumGain;
		#endregion

		internal Channel(int num) {
			Num = num;
			Name = "Channel " + num;
			Init();
		}

		internal void Setup(int sampleRate, int bufferSamples) {
			State = STATE.STANDBY;

			mChoSamples = sampleRate / 10;
			mChoIdx = 0;
			mChoLfoU = 1.0;
			mChoLfoV = -0.5;
			mChoLfoW = -0.5;
			mChoBufferL = new double[mChoSamples];
			mChoBufferR = new double[mChoSamples];

			mDelSamples = sampleRate;
			mDelIdx = 0;
			mDelBufferL = new double[mDelSamples];
			mDelBufferR = new double[mDelSamples];

			DeltaTime = 1.0 / sampleRate;
			BufferSamples = bufferSamples;
			InputBufferL = new double[BufferSamples];
			InputBufferR = new double[BufferSamples];
		}

		internal void SetWaveTable(IntPtr pWaveTable) {
			//WaveTable = pWaveTable;
		}

		internal void NoteOn() {
			if (STATE.STANDBY == State) {
				State = STATE.STARTUP;
			}
		}

		internal void CtrlChange(Ctrl.TYPE type, double value) {
			switch (type) {
			case Ctrl.TYPE.BANK_MSB:
				BankMsb = (byte)value;
				break;
			case Ctrl.TYPE.BANK_LSB:
				BankLsb = (byte)value;
				break;

			case Ctrl.TYPE.MOD:
				VibDepth = VibRange * value;
				break;

			case Ctrl.TYPE.POLTA_TIME:
				break;
			case Ctrl.TYPE.POLTA_SWITCH:
				break;

			case Ctrl.TYPE.VOL:
				Vol = value;
				break;
			case Ctrl.TYPE.EXP:
				Exp = value;
				break;
			case Ctrl.TYPE.PAN:
				Pan = value;
				SetPan();
				break;

			case Ctrl.TYPE.HOLD:
				Hold = value > 0;
				break;

			case Ctrl.TYPE.RESONANCE:
				Resonance = value;
				break;
			case Ctrl.TYPE.CUTOFF:
				Cutoff = value;
				break;

			case Ctrl.TYPE.REV_SEND:
				break;
			case Ctrl.TYPE.CHO_SEND:
				ChoSend = value;
				break;
			case Ctrl.TYPE.DEL_SEND:
				DelSend = value;
				break;

			case Ctrl.TYPE.RESET_ALL_CTRL:
				ResetCTRL();
				break;

			case Ctrl.TYPE.VIB_RANGE:
				VibRange = value;
				break;
			case Ctrl.TYPE.VIB_RATE:
				VibRate = value;
				break;
			case Ctrl.TYPE.VIB_DELAY:
				VibDelay = value;
				break;

			case Ctrl.TYPE.PROG_CHG:
				ProgNum = (byte)((int)value & 0xFF);
				IsDrum = value > 0xFF;
				ProgChange();
				break;

			case Ctrl.TYPE.PITCH:
				Pitch = value;
				break;

			case Ctrl.TYPE.MUTE:
				Mute = value > 0;
				break;

			default:
				break;
			}
		}

		internal void WriteBuffer(double[] pOut, double[] pSpec) {
			for (int i = 0, j=0; i < BufferSamples; i++, j+=2) {
				var outL = InputBufferL[i] * mGain;
				InputBufferL[i] = 0;
				var outR = InputBufferR[i] * mGain;
				InputBufferR[i] = 0;
				mGain += (Vol * Exp - mGain) * 0.1;
				/* コンプレッサー */
				{
				}
				/* イコライザー */
				{
				}
				/* コーラス */
				{
					/* U相V相W相の位置を取得 */
					var posU = mChoIdx - (mChoLfoU * 0.495 + 0.5) * ChoDepth;
					var posV = mChoIdx - (mChoLfoV * 0.495 + 0.5) * ChoDepth;
					var posW = mChoIdx - (mChoLfoW * 0.495 + 0.5) * ChoDepth;
					/* LFOを更新 */
					var choLfoD = ChoRate * PI2_SQRT3 * DeltaTime;
					mChoLfoU += (mChoLfoV - mChoLfoW) * choLfoD;
					mChoLfoV += (mChoLfoW - mChoLfoU) * choLfoD;
					mChoLfoW += (mChoLfoU - mChoLfoV) * choLfoD;
					/* バッファを更新 */
					mChoBufferL[mChoIdx] = outL;
					mChoBufferR[mChoIdx] = outR;
					mChoIdx++;
					mChoIdx -= mChoIdx / mChoSamples * mChoSamples;
					/* U相V相W相の値を線形補間 */
					var idxUa = (int)posU;
					var idxUb = idxUa + 1;
					var idxVa = (int)posV;
					var idxVb = idxVa + 1;
					var idxWa = (int)posW;
					var idxWb = idxWa + 1;
					var a2bU = posU - idxUa;
					var a2bV = posV - idxVa;
					var a2bW = posW - idxWa;
					idxUa -= idxUa / mChoSamples * mChoSamples;
					idxUb -= idxUb / mChoSamples * mChoSamples;
					idxVa -= idxVa / mChoSamples * mChoSamples;
					idxVb -= idxVb / mChoSamples * mChoSamples;
					idxWa -= idxWa / mChoSamples * mChoSamples;
					idxWb -= idxWb / mChoSamples * mChoSamples;
					var choL
						= mChoBufferL[idxUa] * (1 - a2bU) + mChoBufferL[idxUb] * a2bU
						+ mChoBufferL[idxVa] * (1 - a2bV) + mChoBufferL[idxVb] * a2bV
						+ mChoBufferL[idxWa] * (1 - a2bW) + mChoBufferL[idxWb] * a2bW;
					var choR
						= mChoBufferR[idxUa] * (1 - a2bU) + mChoBufferR[idxUb] * a2bU
						+ mChoBufferR[idxVa] * (1 - a2bV) + mChoBufferR[idxVb] * a2bV
						+ mChoBufferR[idxWa] * (1 - a2bW) + mChoBufferR[idxWb] * a2bW;
					/* 出力 */
					outL += choL * ChoSend / 3.0;
					outR += choR * ChoSend / 3.0;
				}
				/* ディレイ */
				{
					/* 遅延した値を取得 */
					var idx = (int)(mDelIdx - DelTime / DeltaTime);
					idx -= idx / mDelSamples * mDelSamples;
					var delL = mDelBufferL[idx];
					var delR = mDelBufferR[idx];
					/* 左右をミックス */
					var mixL = delL * (1 - DelCross) + delR * DelCross;
					var mixR = delR * (1 - DelCross) + delL * DelCross;
					/* バッファを更新 */
					mDelBufferL[mDelIdx] = outL + mixL * DelFeedback;
					mDelBufferR[mDelIdx] = outR + mixR * DelFeedback;
					mDelIdx++;
					mDelIdx -= mDelIdx / mDelSamples * mDelSamples;
					/* 出力 */
					outL += mixL * DelSend;
					outR += mixR * DelSend;
				}
				/* 出力 */
				pOut[j] += outL;
				pOut[j + 1] += outR;
				pSpec[j] += outL * mSpectrumGain;
				pSpec[j + 1] += outR * mSpectrumGain;
				/* メーター */
				{
					RmsL += ((outL * outL) - RmsL) * RMS_SPEED * DeltaTime;
					RmsR += ((outR * outR) - RmsR) * RMS_SPEED * DeltaTime;
					PeakL -= PeakL * PEAK_ATT_DELTA * DeltaTime;
					PeakR -= PeakR * PEAK_ATT_DELTA * DeltaTime;
					if (outL < 0) outL *= -1;
					if (outR < 0) outR *= -1;
					if (PeakL < outL) PeakL = outL;
					if (PeakR < outR) PeakR = outR;
				}
				/* 状態遷移 */
				if (STATE.ACTIVE == State && RmsL < THRESHOLD_STANDBY && RmsR < THRESHOLD_STANDBY) {
					State = STATE.STANDBY;
					break;
				} else if (STATE.STARTUP == State && (RmsL >= THRESHOLD_ACTIVE || RmsR >= THRESHOLD_ACTIVE)) {
					State = STATE.ACTIVE;
				}
			}
			/* 待機状態になったらクリア */
			if (STATE.STANDBY == State) {
				RmsL = 0;
				RmsR = 0;
				PeakL = 0;
				PeakR = 0;
				mChoIdx = 0;
				mChoLfoU = 1.0;
				mChoLfoV = -0.5;
				mChoLfoW = -0.5;
				for (int i = 0; i < mChoSamples; i++) {
					mChoBufferL[i] = 0;
					mChoBufferR[i] = 0;
				}
				mDelIdx = 0;
				for (int i = 0; i < mDelSamples; i++) {
					mDelBufferL[i] = 0;
					mDelBufferR[i] = 0;
				}
				for (int i = 0; i < BufferSamples; i++) {
					InputBufferL[i] = 0;
					InputBufferR[i] = 0;
				}
			}
		}

		void Init() {
			Mute = false;

			IsDrum = false;
			BankMsb = 0;
			BankLsb = 0;
			ProgNum = 0;
			ProgChange();

			Vol = 100 / 127.0;
			Exp = 1;
			mGain = Vol * Exp;
			mSpectrumGain = 1;
			Pan = 0;
			SetPan();

			Hold = false;

			Cutoff = 1;
			Resonance = 0;

			Pitch = 1;

			VibRange = 0.5;
			VibDepth = VibRange;
			VibRate = 4;
			VibDelay = 1.0;

			ChoSend = 0;
			ChoRate = 0.1;
			ChoDepth = 0.01;

			DelSend = 0;
			DelTime = 0.2;
			DelCross = 0;

			RmsL = 0;
			RmsR = 0;
			PeakL = 0;
			PeakR = 0;
		}

		void ResetCTRL() {
			Exp = 1;
			Hold = false;
			VibDepth = VibRange;
			Pitch = 1;
		}

		void SetPan() {
			var th = 0.25 * Math.PI * Pan;
			PanRe = Math.Cos(th);
			PanIm = Math.Sin(th);
		}

		void ProgChange() {

		}
	}
}
