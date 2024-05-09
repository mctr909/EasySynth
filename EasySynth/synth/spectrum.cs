using System;

namespace Synth {
	public class Spectrum {
		struct Bank {
			public double kb0;
			public double ka1;
			public double ka2;
			public double rmsDelta;

			public double l1b1;
			public double l1b2;
			public double l1a1;
			public double l1a2;
			public double l2b1;
			public double l2b2;
			public double l2a1;
			public double l2a2;
			public double r1b1;
			public double r1b2;
			public double r1a1;
			public double r1a2;
			public double r2b1;
			public double r2b2;
			public double r2a1;
			public double r2a2;
			public double rmsL;
			public double rmsR;

			public void Setup(int sampleRate, double freq, double width, double attSpeed) {
				var omega = 2 * Math.PI * freq / sampleRate;
				var s = Math.Sin(omega);
				var alpha = s * Math.Sinh(Math.Log(2) / 2 * width * omega / s);
				var ka0 = alpha + 1;
				kb0 = alpha / ka0;
				ka1 = 2 * Math.Cos(omega) / ka0;
				ka2 = (1 - alpha) / ka0;
				rmsDelta = 0.4 * attSpeed * freq / sampleRate;
			}
		}

		public readonly int BANK_COUNT;

		public double[] AmpL { get; private set; }
		public double[] AmpR { get; private set; }

		const double LIMIT_MIN = 1e-4; /* -80db */
		Bank[] mBank;

		public Spectrum(int sampleRate, int bankCount, int octDiv, double baseFreq, double attSpeed) {
			BANK_COUNT = bankCount;
			AmpL = new double[bankCount];
			AmpR = new double[bankCount];
			mBank = new Bank[bankCount];
			for (int i = 0; i < bankCount; i++) {
				var freq = baseFreq * Math.Pow(2, (double)i / octDiv);
				mBank[i].Setup(sampleRate, freq, 1.0 / octDiv, attSpeed);
			}
		}

		public void Update(double[] input) {
			var samples = input.Length;
			for (int b = 0; b < BANK_COUNT; b++) {
				var bank = mBank[b];
				for (int i = 0; i < samples; i += 2) {
					var bpfL
						= bank.kb0 * input[i]
						- bank.kb0 * bank.l1b2
						- bank.ka1 * bank.l1a1
						- bank.ka2 * bank.l1a2;
					bank.l1b2 = bank.l1b1;
					bank.l1b1 = input[i];
					bank.l1a2 = bank.l1a1;
					bank.l1a1 = bpfL;
					bpfL
						= bank.kb0 * bank.l1a1
						- bank.kb0 * bank.l2b2
						- bank.ka1 * bank.l2a1
						- bank.ka2 * bank.l2a2;
					bank.l2b2 = bank.l2b1;
					bank.l2b1 = bank.l1a1;
					bank.l2a2 = bank.l2a1;
					bank.l2a1 = bpfL;
					var bpfR
						= bank.kb0 * input[i + 1]
						- bank.kb0 * bank.r1b2
						- bank.ka1 * bank.r1a1
						- bank.ka2 * bank.r1a2;
					bank.r1b2 = bank.l1b1;
					bank.r1b1 = input[i + 1];
					bank.r1a2 = bank.l1a1;
					bank.r1a1 = bpfR;
					bpfR
						= bank.kb0 * bank.r1a1
						- bank.kb0 * bank.r2b2
						- bank.ka1 * bank.r2a1
						- bank.ka2 * bank.r2a2;
					bank.r2b2 = bank.r2b1;
					bank.r2b1 = bank.r1a1;
					bank.r2a2 = bank.r2a1;
					bank.r2a1 = bpfR;
					bank.rmsL += (bpfL * bpfL - bank.rmsL) * bank.rmsDelta;
					bank.rmsR += (bpfR * bpfR - bank.rmsR) * bank.rmsDelta;
				}
			}
			for (int b = 0; b < BANK_COUNT; b++) {
				var tempL = Math.Sqrt(mBank[b].rmsL * 2);
				var tempR = Math.Sqrt(mBank[b].rmsR * 2);
				AmpL[b] = 20 * Math.Log10((tempL < LIMIT_MIN) ? LIMIT_MIN : tempL);
				AmpR[b] = 20 * Math.Log10((tempR < LIMIT_MIN) ? LIMIT_MIN : tempR);
			}
		}
	}
}
