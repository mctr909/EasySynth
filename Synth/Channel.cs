using System;
using Synth.Instruments;
using SMF;

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

		public bool Enable { get; private set; }

		public bool IsDrum { get; private set; }
		public byte BankMsb { get; private set; }
		public byte BankLsb { get; private set; }
		public byte ProgNum { get; private set; }

		public bool Hold { get; private set; }

		public double Pitch { get; private set; }

		public double Vol { get; private set; }
		public double Exp { get; private set; }
		public double Pan { get; private set; }

		public double VibDepth { get; private set; }
		public double VibRange { get; private set; }
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
			get { return mSpectrumSwitch > 0; }
			set { mSpectrumSwitch = value ? 1 : 0; }
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
		double mSpectrumSwitch;
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

		internal void CtrlChange(CTRL type, double value) {
			switch (type) {
			case CTRL.BANK_MSB:
				BankMsb = (byte)value;
				break;
			case CTRL.BANK_LSB:
				BankLsb = (byte)value;
				break;

			case CTRL.MOD:
				VibDepth = value;
				break;

			case CTRL.POLTA_TIME:
				break;
			case CTRL.POLTA_SWITCH:
				break;

			case CTRL.VOL:
				Vol = value;
				break;
			case CTRL.PAN:
				SetPan(value);
				break;
			case CTRL.EXP:
				Exp = value;
				break;

			case CTRL.HOLD1:
				Hold = value > 0;
				break;

			case CTRL.RESONANCE:
				Resonance = value;
				break;
			case CTRL.CUTOFF:
				Cutoff = value;
				break;

			case CTRL.VIB_RATE:
				VibRate = value;
				break;
			case CTRL.VIB_DEPTH:
				VibRange = value;
				break;
			case CTRL.VIB_DELAY:
				VibDelay = value;
				break;

			case CTRL.REV_SEND:
				break;
			case CTRL.CHO_SEND:
				ChoSend = value;
				break;
			case CTRL.DEL_SEND:
				DelSend = value;
				break;

			case CTRL.RESET_ALL_CTRL:
				ResetCTRL();
				break;
			case CTRL.LOCAL_CTRL:
				Enable = value > 0;
				break;

			default:
				break;
			}
		}

		internal void ProgChange(int number) {
			ProgNum = (byte)(number & 0x7F);
			IsDrum = number > 0x7F;
		}

		internal void PitchBend(int cent) {
			//Pitch = cent;
			Pitch = 1.0;
		}

		internal unsafe void WriteBuffer(IntPtr output, IntPtr spec) {
			var pOutput = (double*)output;
			var pSpec = (double*)spec;
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
				*pOutput++ += outL;
				*pOutput++ += outR;
				*pSpec++ += outL * mSpectrumSwitch;
				*pSpec++ += outR * mSpectrumSwitch;
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
			Enable = true;

			BankMsb = 0;
			BankLsb = 0;
			ProgChange(0);

			Vol = 100 / 127.0;
			Exp = 1;
			mGain = Vol * Exp;
			mSpectrumSwitch = 1;
			SetPan(0);

			Hold = false;

			Cutoff = 1;
			Resonance = 0;

			PitchBend(0);

			VibDepth = 0;
			VibRange = 0.5;
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
			VibDepth = 0;
			PitchBend(0);
		}

		void SetPan(double pan) {
			var th = 0.25 * Math.PI * pan;
			PanRe = Math.Cos(th);
			PanIm = Math.Sin(th);
			Pan = pan;
		}
	}
}
