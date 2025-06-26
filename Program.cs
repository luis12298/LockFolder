using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace SecureFolder
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        static readonly string PASSWORD = Environment.GetEnvironmentVariable("PASS_FOLDER");

        [STAThread]
        static void Main(string[] args)
        {
            FreeConsole(); // Cierra la consola si se ejecutó en modo consola

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length < 2)
            {
                MessageBox.Show("Uso: securefolder.exe <ruta_de_carpeta> <set/restore>", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderPath = args[0];
            string action = args[1].ToLower();

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("La ruta proporcionada no es una carpeta válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (action == "set")
            {
                SetPermissions(folderPath);
            }
            else if (action == "restore")
            {
                RestorePermissions(folderPath);
            }
            else
            {
                MessageBox.Show("Acción no reconocida. Usa 'set' o 'restore'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void SetPermissions(string folderPath)
        {
            try
            {
                RunCommand("icacls", $"\"{folderPath}\" /inheritance:r");
                RunCommand("icacls", $"\"{folderPath}\" /deny *S-1-1-0:(OI)(CI)F");
                RunCommand("icacls", $"\"{folderPath}\" /deny *S-1-5-32-544:(OI)(CI)F");
                RunCommand("icacls", $"\"{folderPath}\" /deny *S-1-5-18:(OI)(CI)F");

                MessageBox.Show("Carpeta bloqueada completamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al bloquear la carpeta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void RestorePermissions(string folderPath)
        {
            string inputPassword = Interaction.InputBox("Introduce la contraseña para restaurar permisos:", "Restaurar Acceso", "", -1, -1);

            if (inputPassword != PASSWORD)
            {
                MessageBox.Show("Contraseña incorrecta. Se registrará el intento.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.AppendAllText("log.txt", $"Intento fallido en {DateTime.Now}\n");
                return;
            }

            try
            {
                RunCommand("icacls", $"\"{folderPath}\" /inheritance:e");
                RunCommand("icacls", $"\"{folderPath}\" /remove:d *S-1-1-0");
                RunCommand("icacls", $"\"{folderPath}\" /remove:d *S-1-5-32-544");
                RunCommand("icacls", $"\"{folderPath}\" /remove:d *S-1-5-18");
                RunCommand("icacls", $"\"{folderPath}\" /grant *S-1-1-0:(OI)(CI)F");

                MessageBox.Show("Carpeta desbloqueada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar permisos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void RunCommand(string command, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true, // Evita abrir la consola
                        WindowStyle = ProcessWindowStyle.Hidden // Oculta completamente la ventana de la consola

            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
            }
        }
    }
}
