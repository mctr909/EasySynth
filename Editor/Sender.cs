using System.Runtime.InteropServices;

namespace Editor {
    unsafe static class Sender {
        [DllImport("WaveOut.dll")]
        private static extern void waveout_open(
            int sample_rate,
            int buffer_length,
            int buffer_count,
            bool enable_float
        );
        [DllImport("WaveOut.dll")]
        private static extern void waveout_close();
        [DllImport("WaveOut.dll")]
        private static extern void message_send(byte* pMsg);
        [DllImport("WaveOut.dll")]
        private static extern void dls_load(string file_path);

        public static void WaveOpen(int sample_rate = 44100, int buffer_size = 256, int buffer_count = 32, bool enable_float = true) {
            waveout_open(sample_rate, buffer_size, buffer_count, enable_float);
        }

        public static void WaveClose() {
            waveout_close();
        }

        public static void LoadDLS(string path) {
            dls_load(path);
        }

        public static void Messaage(byte[] msg) {
            fixed(byte *p = &msg[0]) {
                message_send(p);
            }
        }
    }
}
