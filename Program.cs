using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Program
    {
        
        static string Message;
        private static bool Continue;
        static string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static void Main(string[] args)
        {
            Continue = true;
            while (Continue)
            {
                writeMenu();
                takeInput();
            }
        }


        private static void writeMenu()
        {
            Console.WriteLine("________________________________________________");
            Console.WriteLine("1: Safely store message");
            Console.WriteLine("2: Read message");
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
                    printMessage();
                }
                else
                {
                    Continue = false;
                }
            }
        }

        private static void encryptMessage()
        {
            Console.Write("Type a message to encrypt: ");
            string input = Console.ReadLine();
            


            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "encryptedMessage.txt")))
            {
                outputFile.WriteLine(input);
            }
        }

        private static void printMessage()
        {
            using (StreamReader file = new StreamReader(Path.Combine(docPath.ToString(), "encryptedMessage.txt")))
            {
                Console.WriteLine(file.ReadLine());
            }
        }
    }
}