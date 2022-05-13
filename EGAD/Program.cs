//EASY GARRYSMOD ASSET DECOUPLER

using Force.Crc32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EGAD
{
    internal class Program
    {

        static string BSPPath = "";
        static string TempFolderName = Path.GetTempPath() + @"EGAD";
        static string LicenseCertificate = @Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\retr0mod8\EGAD\" + @"agreed.dat";
        static byte[] BSPDataPrePakFile;
        [STAThread]
        static void Main(string[] args)
        {
            //if (Directory.Exists(TempFolderName))
            // Directory.Delete(TempFolderName, true);

            //Read License
            if (!File.Exists(LicenseCertificate))
            {
                while (true)
                {
                    ColourPrint("Before continuing, do you accept the program's license?\n(You can ready the license on this program's GitHub page)\n\"yes\" or \"no\"", MessageType.Error);
                    string input = Console.ReadLine().ToLower();
                    if (input == "yes")
                    {
                        try
                        {
                            File.Create(LicenseCertificate);
                        }
                        catch (Exception)
                        {
                            Directory.CreateDirectory(Directory.GetParent(LicenseCertificate).FullName);
                            File.Create(LicenseCertificate);
                        }
                       
                    }
                    else if (input == "no")
                    {
                        ColourPrint("You cannot use the program until you accept the license. I'm sorry :(\nPress enter to exit...", MessageType.Error);
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Incorrect input! Try again");
                        continue;
                    }
                    break;
                }
            }



            //Check if directory exists, prevents data loss
            if (Directory.Exists(TempFolderName))
            {
                while (true)
                {
                    ColourPrint("An existing tempfolder already exists at:\n" + TempFolderName + "\nDo you want it deleted?\n(Once deleted, the files inside are gone forever!)\n\"yes\" or \"no\"", MessageType.Error);
                    string input = Console.ReadLine().ToLower();
                    if (input == "yes")
                    {
                        if (Directory.Exists(TempFolderName))
                            Directory.Delete(TempFolderName, true);
                    }
                    else if (input == "no")
                    {
                        ColourPrint("To protect against the loss of data in that folder, the program shall not launch.\nIf you wish to use this program, please check the folder for any important files, then delete it.\nPress enter to exit...", MessageType.Error);
                        if (Directory.Exists(TempFolderName))
                            Process.Start(TempFolderName);
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Incorrect input! Try again");
                        continue;
                    }
                    break;
                }
            }




            while (true)
            {
                Console.Clear();
                ColourPrint("Select your BSP file...", MessageType.Hint);
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Source Engine BSP Files (*.bsp) | *.bsp";
                DialogResult dr = ofd.ShowDialog();
                BSPPath = ofd.FileName;
                ofd.Dispose();
                Console.Clear();
                if (dr == DialogResult.OK)
                {
                    if (File.Exists(BSPPath))
                    {
                        ColourPrint("File exists, continuing...", MessageType.Hint);
                        break;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("File does not exist! Press enter to try again");
                        Console.ReadLine();
                    }
                }
                else
                {
                    ColourPrint("Dialog Box was closed. Press enter to try again", MessageType.Error);
                    Console.ReadLine();
                }
            }
            try
            {
                while (true)
                {
                    ColourPrint("Do you want to backup the BSP file? (Reccomended)\n\"yes\" or \"no\"", MessageType.Info);
                    string input = Console.ReadLine().ToLower();
                    if (input == "yes")
                    {
                        //Backup system checks to see if backup file already exists, if so just add the next increment from the last one on top.
                        if (File.Exists(BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP.bsp"))
                        {
                            int counter = 0;
                            while (File.Exists(BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP(" + counter + ").bsp"))
                            {
                                counter++;
                            }
                            File.Copy(BSPPath, BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP(" + counter + ").bsp", true);
                            ColourPrint("Backup made at: " + BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP(" + counter + ").bsp", MessageType.Info);
                        }
                        else
                        {
                            File.Copy(BSPPath, BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP.bsp", true);
                            ColourPrint("Backup made at: " + BSPPath.Substring(0, BSPPath.Length - 4) + "-BACKUP.bsp", MessageType.Info);
                        }
                        ColourPrint("Press enter to continue", MessageType.Hint);
                        Console.ReadLine();
                    }
                    else if (input == "no")
                    {
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Incorrect input! Try again");
                        continue;
                    }
                    break;
                }
                RestOfProgram();
            }
            catch (Exception ex)
            {
                ColourPrint("\n\nThe program has run into an issue it can't recover from, details below:\n\n" + ex + "\n\nPlease open 'Snipping Tool' and send a screenshot of this window to retromod8#9627 on Discord.\n\nYour map should still be safe, but please check just in case.\nIf your map is damaged, i apologise.", MessageType.Error);
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        //This is done so exceptions can be easily caught
        static void RestOfProgram()
        {

            FileStream fs = File.OpenRead(BSPPath);
            MemoryStream ms = new MemoryStream();
            BinaryReader br = new BinaryReader(ms);
            fs.CopyTo(ms);
            fs.Dispose();
            ms.Position = 0;
            br.ReadInt32(); //Signature
            int bspversion = br.ReadInt32(); //version
            if (bspversion != 20) //Only gmod supported atm
            {
                Console.WriteLine("BSP version not supported! Press enter to exit");
                Console.ReadLine();
                br.Dispose();
                ms.Dispose();
                Environment.Exit(1);
            }

            //Read lump headers of bsp and store em in an array
            lump_t[] LumpHeaders = new lump_t[64];
            for (int i = 0; i < 64; i++)
            {
                int fileofs = br.ReadInt32();
                int filelen = br.ReadInt32();
                int version = br.ReadInt32();
                char[] fourCC = br.ReadChars(4);
                LumpHeaders[i] = new lump_t(fileofs, filelen, version, fourCC);
            }

            BSPDataPrePakFile = new byte[LumpHeaders[40].fileofs]; //Setup array
            ms.Position = 0;
            ms.Read(BSPDataPrePakFile, 0, LumpHeaders[40].fileofs); //Copy all data before pakfile lump
            ms.Position = LumpHeaders[40].fileofs; //Set position to pakfile position

            Directory.CreateDirectory(TempFolderName); //Make the temp folder

            //The while loop below reads out all pakfile data until reach PK12 which contains no data for each file
            FileStream output;
            while (true)
            {
                br.ReadUInt16(); //Read PK
                byte ID1 = br.ReadByte();
                byte ID2 = br.ReadByte();
                if (ID1 == 3 && ID2 == 4)
                {
                    br.ReadUInt16(); // versionneeded
                    br.ReadUInt16(); // flags
                    br.ReadUInt16(); // cmethod
                    br.ReadUInt16(); // lasttime
                    br.ReadUInt16(); // lastdate
                    uint crc = br.ReadUInt32(); // crc
                    br.ReadInt32(); // csize
                    int datasize = br.ReadInt32(); // usize
                    ushort namelength = br.ReadUInt16(); // filenamelength
                    br.ReadUInt16(); // extralength
                    string filename = new string(br.ReadChars(namelength));
                    byte[] data = br.ReadBytes(datasize);
                    try
                    {
                        output = File.Create(TempFolderName + @"\" + filename);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            //Failsafe in the event a file is named something weird
                            Directory.CreateDirectory(TempFolderName + @"\" + filename.Substring(0, filename.LastIndexOf('/')));
                            output = File.Create(TempFolderName + @"\" + filename);
                        }
                        catch (Exception)
                        {
                            //Final failsafe
                            ColourPrint("INVALID FILE PATH:\n\t" + filename, MessageType.Error);
                            continue;
                        }

                    }
                    output.Position = 0;
                    output.Write(data, 0, data.Length);
                    output.Dispose();
                }
                else
                    break;
            }
            ms.Dispose();

            Process.Start(TempFolderName); //Open the temp folder
            int NumberOfFiles = Directory.GetFiles(TempFolderName, "*", SearchOption.AllDirectories).Length;

            //Wait for user to input the command to continue
            while (true)
            {
                ColourPrint(NumberOfFiles + " file(s) extracted!", MessageType.Info);
                ColourPrint("Enter the word \"ready\" when you're ready to update the BSP.", MessageType.Hint);
                ColourPrint("Enter the word \"exit\" to exit the program.", MessageType.Error);
                string input = Console.ReadLine().ToLower();
                if (input == "ready")
                    break;
                else if (input == "exit")
                {
                    if (Directory.Exists(TempFolderName))
                        Directory.Delete(TempFolderName, true);
                    ColourPrint("Deleting temp folder...", MessageType.Hint);
                    Environment.Exit(1);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Incorrect input! Try again");
                }
            }


            //Get all files in temp folder
            string[] TempFolderIndexs = Directory.GetFiles(TempFolderName, "*", SearchOption.AllDirectories);

            //Add each file in the directory to a list with both the Windows FS path and Source FS Path
            //The for loop below is not neccesary and a workaround should be done


            List<TempPathFile> InputFiles = new List<TempPathFile>(); //Store info about files in temp folder
            for (int i = 0; i < TempFolderIndexs.Length; i++)
            {
                InputFiles.Add(new TempPathFile(TempFolderIndexs[i], TempFolderIndexs[i].Substring(TempFolderName.Length + 1).Replace('\\', '/')));
            }
            List<PAKFILE> PAKFILEENTRIES = new List<PAKFILE>(); //Store data for the files
            for (int i = 0; i < InputFiles.Count; i++)
            {
                byte[] Data = File.ReadAllBytes(InputFiles[i].FilePath);
                PAKFILEENTRIES.Add(new PAKFILE(InputFiles[i].SourceFSPath, Data, GetHash(Data)));
            }


            //Creation of the output BSP

            FileStream outbsp = File.Create(BSPPath);
            BinaryWriter bw = new BinaryWriter(outbsp);
            outbsp.Position = 0;
            outbsp.Write(BSPDataPrePakFile, 0, BSPDataPrePakFile.Length); //Write all the same data as the original, except for the pakfile lump
            //outbsp.Write(pakdata, 0, pakdata.Length);
            int PK34Size = 0; //Counters used both for the pk12 offset variable (relativeOffsetOfLocalHeader) and the pk56 footer
            int PK1256Size = 0; //Variable used for the pk56 footer

            //For each file in the temp directory...
            for (int i = 0; i < PAKFILEENTRIES.Count; i++)
            {
                PAKFILEENTRIES[i].Offset = Convert.ToUInt32(PK34Size);
                Console.WriteLine("Writing PK34 for " + PAKFILEENTRIES[i].filepath + "\t FileSize: " + PAKFILEENTRIES[i].data.Length + "  " + "(" + (float)PAKFILEENTRIES[i].data.Length / 1000 + " kb)");
                bw.Write("PK".ToCharArray());
                bw.Write((byte)3);
                bw.Write((byte)4);
                bw.Write((short)0); //version
                bw.Write((short)0); //flags
                bw.Write((short)0); //cmethod
                bw.Write((short)0); //lasttime
                bw.Write((short)0); //lastdate
                bw.Write(PAKFILEENTRIES[i].crc);
                bw.Write(PAKFILEENTRIES[i].data.Length);
                bw.Write(PAKFILEENTRIES[i].data.Length);
                bw.Write(Convert.ToUInt16(PAKFILEENTRIES[i].filepath.Length));
                bw.Write((short)0);
                bw.Write(PAKFILEENTRIES[i].filepath.ToCharArray());
                outbsp.Write(PAKFILEENTRIES[i].data, 0, PAKFILEENTRIES[i].data.Length);
                PK34Size += 30 + PAKFILEENTRIES[i].filepath.ToCharArray().Length + PAKFILEENTRIES[i].data.Length;
            }
            for (int i = 0; i < PAKFILEENTRIES.Count; i++)
            {
                Console.WriteLine("Writing PK12 for " + PAKFILEENTRIES[i].filepath);
                bw.Write("PK".ToCharArray());
                bw.Write((byte)1);
                bw.Write((byte)2);
                bw.Write((short)0); //versionMadeBy
                bw.Write((short)0); //versionNeededToExtract
                bw.Write((short)0);//flags
                bw.Write((short)0);//compressionMethod
                bw.Write((short)0);//lastModifiedTime
                bw.Write((short)0);//lastModifiedDate
                bw.Write(PAKFILEENTRIES[i].crc);
                bw.Write(PAKFILEENTRIES[i].data.Length);
                bw.Write(PAKFILEENTRIES[i].data.Length);
                bw.Write(Convert.ToUInt16(PAKFILEENTRIES[i].filepath.Length));
                bw.Write((short)0);//extraFieldLength
                bw.Write((short)0);//fileCommentLength
                bw.Write((short)0);//diskNumberStart
                bw.Write((short)0);//internalFileAttribs
                bw.Write((int)0);//externalFileAttribs
                bw.Write(PAKFILEENTRIES[i].Offset);//relativeOffsetOfLocalHeader
                bw.Write(PAKFILEENTRIES[i].filepath.ToCharArray());
                PK1256Size += 46 + PAKFILEENTRIES[i].filepath.ToCharArray().Length;
            }
            Console.WriteLine("Writing PK56 Footer");
            bw.Write("PK".ToCharArray());
            bw.Write((byte)5);
            bw.Write((byte)6);
            bw.Write((short)0);//numberOfThisDisk
            bw.Write((short)0);//numberOfTheDiskWithStartOfCentralDirectory
            bw.Write(Convert.ToUInt16(PAKFILEENTRIES.Count));//nCentralDirectoryEntries_ThisDisk
            bw.Write(Convert.ToUInt16(PAKFILEENTRIES.Count));//nCentralDirectoryEntries_Total
            bw.Write(PK1256Size);//PK1256
            bw.Write(PK34Size);//PK34
            bw.Write((short)0);//commentLength
            outbsp.Position = 652;
            bw.Write(PK1256Size + PK34Size + 22);

            /*Round filesize up to nearest 2 bytes, idk why bsp does this?
             * Maybe a leftover of quake considering this might've been
             * useful for FAT filesystems maybe?
            */
            int stufftoadd = Convert.ToInt32(outbsp.Length) % 2;
            for (int i = 0; i < stufftoadd; i++)
            {
                outbsp.WriteByte(0);
            }

            bw.Dispose();
            outbsp.Dispose();
            Directory.Delete(TempFolderName, true);
            ColourPrint("Total Files: " + PAKFILEENTRIES.Count, MessageType.Info);
            ColourPrint("PK34 Size: " + PK34Size + "  " + "(" + (float)PK34Size / 1000 + "kb)" + "  " + "(" + (float)PK34Size / 1000000 + " mb)", MessageType.Info);
            ColourPrint("PK12 Size: " + PK1256Size + "  " + "(" + (float)PK1256Size / 1000 + "kb)" + "  " + "(" + (float)PK1256Size / 1000000 + " mb)", MessageType.Info);

            ColourPrint("\n\nBSP Successfully Modified!\nBSP Path: " + BSPPath, MessageType.Success);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            Environment.Exit(0);
        }

        class lump_t
        {
            internal int fileofs;
            internal int filelen;
            internal int version;
            internal char[] fourCC;

            internal lump_t(int fileofs, int filelen, int version, char[] fourCC)
            {
                this.fileofs = fileofs;
                this.filelen = filelen;
                this.version = version;
                this.fourCC = fourCC;
            }
        }

        class PAKFILE
        {
            internal string filepath;
            internal byte[] data;
            internal uint crc;
            internal uint Offset = 0;

            internal PAKFILE(string filepath, byte[] data, uint crc)
            {
                this.filepath = filepath;
                this.data = data;
                this.crc = crc;
            }
        }

        //Simple CRC32 algorithm
        internal static uint GetHash(byte[] Input)
        {
            Crc32Algorithm crc32Algorithm = new Crc32Algorithm();

            var hash = String.Empty;

            foreach (byte b in crc32Algorithm.ComputeHash(Input))
                hash += b.ToString("x2").ToLower();
            return Convert.ToUInt32(hash, 16);
        }

        class TempPathFile
        {
            internal string FilePath;
            internal string SourceFSPath;
            internal TempPathFile(string FilePath, string SourceFSPath)
            {
                this.FilePath = FilePath;
                this.SourceFSPath = SourceFSPath;
            }
        }

        //Used for colour text
        internal enum MessageType
        {
            Error,
            Hint,
            Info,
            Success
        }
        //Ditto
        internal static void ColourPrint(string Message, MessageType Type)
        {
            ConsoleColor CurrentText = Console.ForegroundColor;
            ConsoleColor CurrentBack = Console.BackgroundColor;
            switch (Type)
            {
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;

                case MessageType.Hint:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Blue;
                    break;

                case MessageType.Info:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    break;

                case MessageType.Success:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Green;
                    break;

                default:
                    break;
            }
            Console.WriteLine(Message);
            Console.ForegroundColor = CurrentText;
            Console.BackgroundColor = CurrentBack;
        }
    }
}
