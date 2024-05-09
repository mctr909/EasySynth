using System;

namespace Synth {
	public class Spectrum {
		class Bank {
			public readonly double Kb0;
			public readonly double Ka1;
			public readonly double Ka2;
			public readonly double Freq;

			public double RmsDelta;

			public double Lb1;
			public double Lb2;
			public double La1;
			public double La2;
			public double RmsL;

			public double Rb1;
			public double Rb2;
			public double Ra1;
			public double Ra2;
			public double RmsR;

			public Bank(int sampleRate, double freq, double width) {
				var omega = 2 * Math.PI * freq / sampleRate;
				var s = Math.Sin(omega);
				var alpha = s * Math.Sinh(Math.Log(2) / 2 * width * omega / s);
				var ka0 = alpha + 1;
				Kb0 = alpha / ka0;
				Ka1 = 2 * Math.Cos(omega) / ka0;
				Ka2 = (1 - alpha) / ka0;
				Freq = freq;
			}
		}

		public readonly int SampleRate;
		public readonly int BankCount;

		const double LimitMin = 1e-4; /* -80db */

		public double[] AmpL { get; private set; }
		public double[] AmpR { get; private set; }

		Bank[] mBank;

		public Spectrum(int sampleRate, int bankCount, int octDiv, double baseFreq) {
			SampleRate = sampleRate;
			BankCount = bankCount;
			AmpL = new double[BankCount];
			AmpR = new double[BankCount];
			mBank = new Bank[BankCount];
			for (int i = 0; i < BankCount; i++) {
				var freq = baseFreq * Math.Pow(2, (double)i / octDiv);
				mBank[i] = new Bank(SampleRate, freq, 1.0 / octDiv);
			}
			SetResponceSpeed(20);
		}

		public void SetResponceSpeed(double speed) {
			for (int i = 0; i < BankCount; i++) {
				var bank = mBank[i];
				if (bank.Freq < speed) {
					bank.RmsDelta = bank.Freq / SampleRate;
				} else {
					bank.RmsDelta = speed / SampleRate;
				}
			}
		}

		public void Update(double[] input) {
			var samples = input.Length;
			for (int b = 0; b < BankCount; b++) {
				var bank = mBank[b];
				for (int i = 0; i < samples; i += 2) {
					var bpfL
						= bank.Kb0 * input[i]
						- bank.Kb0 * bank.Lb2
						- bank.Ka1 * bank.La1
						- bank.Ka2 * bank.La2;
					bank.Lb2 = bank.Lb1;
					bank.Lb1 = input[i];
					bank.La2 = bank.La1;
					bank.La1 = bpfL;
					bank.RmsL += (bpfL * bpfL - bank.RmsL) * bank.RmsDelta;
					var bpfR
						= bank.Kb0 * input[i + 1]
						- bank.Kb0 * bank.Rb2
						- bank.Ka1 * bank.Ra1
						- bank.Ka2 * bank.Ra2;
					bank.Rb2 = bank.Lb1;
					bank.Rb1 = input[i + 1];
					bank.Ra2 = bank.La1;
					bank.Ra1 = bpfR;
					bank.RmsR += (bpfR * bpfR - bank.RmsR) * bank.RmsDelta;
				}
			}
			for (int b = 0; b < BankCount; b++) {
				var tempL = Math.Sqrt(mBank[b].RmsL * 2);
				var tempR = Math.Sqrt(mBank[b].RmsR * 2);
				AmpL[b] = 20 * Math.Log10((tempL < LimitMin) ? LimitMin : tempL);
				AmpR[b] = 20 * Math.Log10((tempR < LimitMin) ? LimitMin : tempR);
			}
		}
	}
}
