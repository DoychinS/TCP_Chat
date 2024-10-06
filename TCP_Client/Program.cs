using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private const int Port = 8888;
    private const string ServerIp = "127.0.0.1"; // IP адрес на сървъра (localhost)

    static void Main()
    {
        TcpClient client = new TcpClient(ServerIp, Port); // Създава TCP клиент и го свързва към сървъра на зададения IP адрес и порт
        Console.WriteLine("Connected to server. Start chatting!");

        NetworkStream stream = client.GetStream();// Създава поток за комуникация с отдалечения сървър

        Thread receiveThread = new Thread(ReceiveMessages);// Създава и стартира нов поток за получаване на съобщения от сървъра
        receiveThread.Start(stream);// Предава потока като аргумент на нишката

        while (true)// Основен цикъл за изпращане на съобщения към сървъра
        {
            string message = Console.ReadLine();// Чете съобщение от конзолата
            byte[] buffer = Encoding.ASCII.GetBytes(message);// Преобразува текста в байтове
            stream.Write(buffer, 0, buffer.Length);// Изпраща байтовете през потока към сървъра
        }
    }

    static void ReceiveMessages(object obj)
    {
        NetworkStream stream = (NetworkStream)obj;// Преобразува аргумента в NetworkStream
        byte[] buffer = new byte[1024];// Буфер за получаване на данни
        int bytesRead;

        while (true)// Непрекъснат цикъл за четене на съобщения от сървъра
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);// Чете байтове от потока
                if (bytesRead == 0)// Проверява дали връзката е прекъсната
                {
                    break;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);// Преобразува получените байтове в текстово съобщение
                Console.WriteLine(message);// Извежда съобщението на конзолата
            }
            catch (Exception)
            {
                break;// При грешка (например прекъсната връзка) излиза от цикъла
            }
        }
    }
}
