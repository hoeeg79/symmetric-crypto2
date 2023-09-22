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
        
        // generate a key
        static byte[] key;
        
        static void Main(string[] args)
        {
            Continue = true;
            IntroScreen();
            while (Continue)
            {
                writeMenu();
                takeInput();
            }
        }

        private static string encrypt(string passphrase, int choice)
        {
            using var aes = new AesGcm(key);

            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // MaxSize = 12
            RandomNumberGenerator.Fill(nonce);

            var plaintextBytes = Encoding.UTF8.GetBytes(passphrase);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];

            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            string b64String = Convert.ToBase64String(ciphertext);

            if (choice == 1)
            {
                return b64String;
            }

            if (choice == 3)
            {
                using (var aes2 = new AesGcm(key))
                {
                    plaintextBytes = new byte[ciphertext.Length];
                    aes2.Decrypt(nonce, ciphertext, tag, plaintextBytes);
                    return Encoding.UTF8.GetString(plaintextBytes);
                }
            }

            return null;
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
                    encryptMessage();
                } 
                else if (typed == 2 && typed != null)
                {
                    printEncryptedMessage();
                }
                else if (typed == 3 && typed != null)
                {
                    printDecryptedMessage();
                }
                else
                {
                    Continue = false;
                }
            }
        }

        private static void printDecryptedMessage()
        {
            using (StreamReader file = new StreamReader(Path.Combine(docPath.ToString(), "encryptedMessage.txt")))
            {
                string output = encrypt(file.ReadLine(), 3);
                Console.WriteLine(output);
            }
        }

        private static void encryptMessage()
        {
            Console.Write("Type a message to encrypt: ");
            string input = Console.ReadLine();

            string encrypted = encrypt(input, 1);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "encryptedMessage.txt")))
            {
                
                outputFile.WriteLine(encrypted);
            }
        }

        private static void printEncryptedMessage()
        {
            using (StreamReader file = new StreamReader(Path.Combine(docPath.ToString(), "encryptedMessage.txt")))
            {
                Console.WriteLine(file.ReadLine());
            }
        }

        private static void IntroScreen()
        {
            Console.Write("Type a passphrase: ");
            
            // Use a KDF (e.g., PBKDF2) to derive a 32-byte key
            byte[] salt = Encoding.UTF8.GetBytes("SomeSaltValue"); // Provide a salt value
            int iterations = 10000; // Number of iterations (adjust as needed)

            using (var deriveBytes = new Rfc2898DeriveBytes(Console.ReadLine(), salt, iterations))
            {
                key = deriveBytes.GetBytes(32); // Generate a 32-byte key
            }
        }
    }
}