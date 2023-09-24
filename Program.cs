using System;
using System.Text;
using System.Security.Cryptography;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Program
    {
        
        static string Message;
        private static bool Continue;
        static string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static byte[] Key;
        private static int NonceSizeInBytes = 12; // Adjust the nonce size as needed
        private static int TagSizeInBytes = 16;   // Fixed tag size of 128 bits (16 bytes)

        
        static void Main(string[] args)
        {
            Continue = true;
            GenerateKey();
            while (Continue)
            {
                writeMenu();
                takeInput();
            }
        }

        private static string EncryptMessage(string message, byte[] key)
        {
            // initialization of AES-GCM to encrypt data
            using (AesGcm aesGcm = new AesGcm(key))
            {
                // Generate nonce
                byte[] nonce = new byte[NonceSizeInBytes];
                RandomNumberGenerator.Fill(nonce);

                // Convert message to byte array - preparing it for encryption
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(message);
                // Cipher byte array created as the same size as the message array above.
                // Used for storing the encrypted message
                byte[] ciphertextBytes = new byte[plaintextBytes.Length];
                // Fixed tag size
                byte[] tag = new byte[TagSizeInBytes];
                
                // Magic happens. After this line, the ciphertextBytes and tag variable will contain
                // the encrypted message, and authentication tag.
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

                // new byte array to store the encrypted message, it will contain all needed to decrypt, except the key.
                byte[] encryptedMessage = new byte[NonceSizeInBytes + ciphertextBytes.Length + TagSizeInBytes];
                
                // inserts nonce
                nonce.CopyTo(encryptedMessage, 0);
                // inserts ciphertext
                ciphertextBytes.CopyTo(encryptedMessage, NonceSizeInBytes);
                //inserts authentication tag
                tag.CopyTo(encryptedMessage, NonceSizeInBytes + ciphertextBytes.Length);

                // return a base64 String of the byte-array containing nonce, cipher and tag.
                return Convert.ToBase64String(encryptedMessage);
            }
        }

        private static string DecryptMessage(string encryptedMessage, byte[] key)
        {
            // initialization of AES-GCM to encrypt data
            using (AesGcm aesGcm = new AesGcm(key))
            {
                // converts the encrypted string to a byte array - preparing for decryption
                byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);

                // Checks recieved message is at least the size of nonce + tag
                if (encryptedBytes.Length < NonceSizeInBytes + TagSizeInBytes)
                {
                    throw new ArgumentException("Invalid encrypted message format");
                }

                // arrays to store nonce, tag and ciphertext.
                byte[] nonce = new byte[NonceSizeInBytes];
                byte[] tag = new byte[TagSizeInBytes];
                byte[] ciphertextBytes = new byte[encryptedBytes.Length - NonceSizeInBytes - TagSizeInBytes];

                // extracting nonce from the encrypted message array
                Array.Copy(encryptedBytes, nonce, NonceSizeInBytes);
                // extracting ciphertext from encrypted message array
                Array.Copy(encryptedBytes, NonceSizeInBytes, ciphertextBytes, 0, ciphertextBytes.Length);
                // extracting tag from encrypted message array
                Array.Copy(encryptedBytes, NonceSizeInBytes + ciphertextBytes.Length, tag, 0, TagSizeInBytes);

                // array to store the decrypted message
                byte[] plaintextBytes = new byte[ciphertextBytes.Length];
                
                // magic happens, using the nonce, ciphertext and tag, the original message is inserted
                // into the byte array created above
                aesGcm.Decrypt(nonce, ciphertextBytes, tag, plaintextBytes);

                // converting byte array to string
                return Encoding.UTF8.GetString(plaintextBytes);
            }
        }

        private static void writeMenu()
        {
            Console.WriteLine("________________________________________________");
            Console.WriteLine("1: Safely store message");
            Console.WriteLine("2: Show encrypted message");
            Console.WriteLine("3: Show decrypted message");
            Console.WriteLine("0: Exit");
        }

        private static void takeInput()
        {
            Console.Write("Choose an option from above: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int typed))
            {
                if (typed == 1)
                {
                    SaveToFile();
                } 
                else if (typed == 2)
                {
                    printEncryptedMessage();
                }
                else if (typed == 3)
                {
                    PrintDecryptedMessage();
                }
                else
                {
                    Continue = false;
                }
            }
        }

        private static void SaveToFile()
        {
            Console.Write("Type a message to encrypt: ");
            string input = Console.ReadLine()!;

            string encrypted = EncryptMessage(input, Key);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "encryptedMessage.txt")))
            {
                
                outputFile.WriteLine(encrypted);
            }
        }

        private static void PrintDecryptedMessage()
        {
            using (StreamReader file = new StreamReader(Path.Combine(docPath.ToString(), "encryptedMessage.txt")))
            {
                string input = file.ReadLine()!;
                
                string output = DecryptMessage(input, Key);
                Console.WriteLine(output);
            }
        }

        private static void printEncryptedMessage()
        {
            using (StreamReader file = new StreamReader(Path.Combine(docPath.ToString(), "encryptedMessage.txt")))
            {
                Console.WriteLine(file.ReadLine());
            }
        }

        private static void GenerateKey()
        {
            Console.Write("Type a passphrase: ");
            
            // Use a KDF (e.g., PBKDF2) to derive a 32-byte key
            byte[] salt = Encoding.UTF8.GetBytes("SomeSaltValue"); // Provide a salt value
            int iterations = 10000; // Number of iterations (adjust as needed)

            using (var deriveBytes = new Rfc2898DeriveBytes(Console.ReadLine(), salt, iterations))
            {
                Key = deriveBytes.GetBytes(32); // Generate a 32-byte key
            }
        }
    }
}