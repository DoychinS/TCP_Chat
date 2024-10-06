using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static readonly List<TcpClient> clients = new List<TcpClient>();// Списък за съхранение на свързаните клиенти
    private const int Port = 8888;// Порт, на който сървърът ще слуша за входящи връзки

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, Port);// Създаване на TCP сървър, който ще слуша на всеки достъпен IP адрес и на зададения порт
        server.Start();// Стартиране на сървъра
        Console.WriteLine($"Server started on port {Port}");

        while (true)// Непрекъснато приемане на клиенти
        {
            TcpClient client = server.AcceptTcpClient();// Приема нов клиент
            clients.Add(client);// Добавя клиента в списъка на свързаните клиенти
            Thread clientThread = new Thread(HandleClient);// Създава нова нишка за клиента
            clientThread.Start(client);// Стартира нишката за клиента
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient tcpClient = (TcpClient)obj;// Преобразува аргумента в TcpClient
        NetworkStream stream = tcpClient.GetStream();// Създава поток за комуникация с клиента

        byte[] buffer = new byte[1024];// Буфер за получаване на данни
        int bytesRead;

        while (true)// Цикъл за получаване и разпращане на съобщенията на клиента
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);// Чете данни от клиента
                if (bytesRead == 0)// Ако клиентът прекъсне връзката
                {
                    break;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);// Преобразува байтовете в текстово съобщение
                Console.WriteLine($"Received: {message}");// Извежда съобщението на сървъра

                BroadcastMessage(tcpClient, message);// Изпраща съобщението до всички останали клиенти
            }
            catch (Exception)
            {
                break;// При грешка (например прекъсната връзка) излиза от цикъла
            }
        }

        clients.Remove(tcpClient);// Премахване на клиента от списъка при прекъсване на връзката
        tcpClient.Close();// Затваряне на връзката с клиента
    }

    static void BroadcastMessage(TcpClient sender, string message)
    {
        byte[] broadcastBuffer = Encoding.ASCII.GetBytes(message);// Преобразува съобщението в байтове

        foreach (TcpClient client in clients)// Изпраща съобщението до всички свързани клиенти, освен изпращача
        {
            if (client != sender)// Не изпраща съобщението обратно на изпращача
            {
                NetworkStream stream = client.GetStream();
                stream.Write(broadcastBuffer, 0, broadcastBuffer.Length);// Изпраща съобщението
            }
        }
    }
}
