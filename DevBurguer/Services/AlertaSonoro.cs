using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace DevBurguer.Services
{
    /// <summary>
    /// Toca um alerta sonoro agradável (estilo notificação de app de delivery),
    /// sintetizado em memória. Não depende de arquivos externos nem do
    /// Console.Beep (que é áspero). Um arpejo maior curto, com timbre suave
    /// tipo marimba (decaimento exponencial + 2º harmônico).
    /// </summary>
    public static class AlertaSonoro
    {
        private static byte[] _wav;
        private static readonly object _lock = new object();

        /// <summary>Toca o alerta sem travar a interface.</summary>
        public static void Tocar()
        {
            try
            {
                byte[] bytes = ObterWav();
                Task.Run(() =>
                {
                    try
                    {
                        using (var ms = new MemoryStream(bytes))
                        using (var player = new SoundPlayer(ms))
                            player.PlaySync();
                    }
                    catch { Fallback(); }
                });
            }
            catch { Fallback(); }
        }

        private static void Fallback()
        {
            try { SystemSounds.Exclamation.Play(); } catch { }
        }

        private static byte[] ObterWav()
        {
            if (_wav != null) return _wav;
            lock (_lock)
            {
                if (_wav == null) _wav = GerarWav();
                return _wav;
            }
        }

        private static byte[] GerarWav()
        {
            const int sampleRate = 44100;

            // Arpejo maior alegre: Lá, Dó#, Mi, Lá agudo (acorde de Lá maior)
            var notas = new (double freq, double dur)[]
            {
                (880.00,  0.12), // A5
                (1108.73, 0.12), // C#6
                (1318.51, 0.12), // E6
                (1760.00, 0.26), // A6 (mais longa, fecha o toque)
            };

            var amostras = new List<short>();

            // pequeno silêncio inicial (~20 ms)
            int silencio = (int)(sampleRate * 0.02);
            for (int i = 0; i < silencio; i++) amostras.Add(0);

            foreach (var (freq, dur) in notas)
            {
                int n = (int)(sampleRate * dur);
                for (int i = 0; i < n; i++)
                {
                    double t = (double)i / sampleRate;
                    // envelope: ataque rápido + decaimento exponencial (timbre marimba)
                    double env = Math.Exp(-5.0 * t / dur);
                    // onda: fundamental + 2º harmônico suave
                    double s = Math.Sin(2 * Math.PI * freq * t)
                             + 0.30 * Math.Sin(2 * Math.PI * freq * 2 * t);
                    double v = s * env * 0.35; // volume
                    if (v > 1.0) v = 1.0; else if (v < -1.0) v = -1.0;
                    amostras.Add((short)(v * short.MaxValue));
                }
            }

            return MontarWav(amostras.ToArray(), sampleRate);
        }

        private static byte[] MontarWav(short[] amostras, int sampleRate)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                int dataBytes = amostras.Length * 2;

                bw.Write(Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataBytes);
                bw.Write(Encoding.ASCII.GetBytes("WAVE"));

                bw.Write(Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);             // tamanho do bloco fmt
                bw.Write((short)1);       // PCM
                bw.Write((short)1);       // canais (mono)
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2); // byte rate (mono, 16 bits)
                bw.Write((short)2);       // block align
                bw.Write((short)16);      // bits por amostra

                bw.Write(Encoding.ASCII.GetBytes("data"));
                bw.Write(dataBytes);
                foreach (short s in amostras) bw.Write(s);

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}
