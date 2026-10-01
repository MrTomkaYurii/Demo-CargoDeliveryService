using FakeDataDriverDbGenerator.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using Bogus.Bson;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8; 
        bool exit = false;
        while (!exit)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=======================");
            Console.WriteLine("ВИБЕРІТЬ ЗАДАЧУ:");
            Console.WriteLine("1. Видалення бази даних");
            Console.WriteLine("2. Створення файлу міграцій");
            Console.WriteLine("3. Розгортання бази даних");
            Console.WriteLine("4. Вихід");
            Console.WriteLine("=======================");

            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await Task.Run(() => DeleteDatabase());
                    break;
                case "2":
                    await Task.Run(() => CreateMigration());
                    break;
                case "3":
                    await Task.Run(() => CreateDatabase());
                    break;
                case "4":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Некоректне виведення");
                    break;
            }
           
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }
    }

    /// <summary>
    /// ВИДАЛЕННЯ БАЗИ ДАНИХ
    /// </summary>
    static void DeleteDatabase()
    {
        Console.Clear();
        Console.WriteLine("Task is started...\n");
        Task.Delay(1000).Wait();

        Console.WriteLine("==================");

        Console.Write("Введіть ім'я серверу, на якому необхідно видалити: \n");
        string? serverName = Console.ReadLine()?.ToLower();
        Console.Write("Введіть ім'я бази даних, яку необхідно видалити: \n");
        string? databaseName = Console.ReadLine()?.ToLower();
        
        string connectionString;

        // Перевіряєм, чи є у користувача логін та пароль для бази даних
        Console.WriteLine("Чи встановлено на базі даних логін та пароль? (y/n): ");
        string? response = Console.ReadLine()?.ToLower();

        if (response == "y")
        {
            Console.WriteLine("Введіть логін: \n");
            string? username = Console.ReadLine()?.ToLower();
            Console.WriteLine("Введіть пароль: \n");
            string? password = Console.ReadLine()?.ToLower();
            // Створення рядка підключенння до SQL серверу із логіном та паролем
            connectionString = $"Server={serverName};Database={databaseName};Trusted_Connection=True;User Id={username};Password={password};";

        }
        else
        {
            // Створення рядка підключення до SQL серверу без лоогіну та паролю
            connectionString = $"Server={serverName};Database={databaseName};Trusted_Connection=True;";
        }

        Console.WriteLine($"Рядок підключення до БД : {connectionString} \n");
        Task.Delay(500).Wait();
        Console.WriteLine($"Видаляю базу даних {databaseName} \n");

        // Створення контексту бази даних
        using (var dbContext = new DbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options))
        {
            // Перевірка існування бази даних
            bool databaseExists = dbContext.Database.CanConnect();
            if (databaseExists)
            {
                // Видалення бази даних
                dbContext.Database.EnsureDeleted();
                Console.WriteLine($"База даних {databaseName} успішно видалена. \n");
            }
            else
            {
                Console.WriteLine($"База даних {databaseName} не знайдена. \n");
            }
        }
        
        Console.WriteLine("Task is finished...\n");
        Task.Delay(2000).Wait();
        Console.WriteLine("Press Enter to continue...\n");
    }

    /// <summary>
    /// Створення файлу міграції 
    /// </summary>
    static void CreateMigration()
    {
        // Отримуємо поточну директорію
        string currentDirectory = Directory.GetCurrentDirectory();
        //Шукаємо шлях до кореню проекту
        var projectName = FindFileWithExtension(currentDirectory, "csproj");
        // Запитуємось у користувача ім'я файлу міграції
        Console.Write("Введіть ім'я міграції: ");
        string? migrationName = Console.ReadLine();
        // Створюємо клас міграції з допомогою командного рядка операційної системи
        string command = $"dotnet ef migrations add {migrationName} --project {projectName} --no-build";
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/c {command}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        // Виводимо результат виконання команди
        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);
        // Очікуємо завершення процесу
        process.WaitForExit();
        Console.WriteLine("Press Enter to continue...");
    }

    /// <summary>
    /// РОЗГОРТАННЯ БАЗИ ДАНИХ
    /// </summary>
    static void CreateDatabase()
    {
        Console.Clear();
        Console.WriteLine("Task is started...\n");
        Task.Delay(1000).Wait();

        Console.WriteLine("==================");
        Console.Write("Введіть ім'я серверу: \n");
        string? serverName = Console.ReadLine();

        Console.Write("Введыть ім'я бази даних: \n");
        string? databaseName = Console.ReadLine();

        Console.Write("Чи хочете ви використовувати логін ты пароль для доступу до бази даних? (y/n): \n");
        string? useCredentials = Console.ReadLine();

        string connectionString;
        if (useCredentials?.ToLower() == "y")
        {
            Console.Write("Введіть ім'я користувача: \n");
            string? userName = Console.ReadLine();

            Console.Write("Введіть пароль: \n");
            string password = ReadPassword();

            connectionString = $"Server={serverName};Database={databaseName};User Id={userName};Password={password};Integrated Security=true;TrustServerCertificate=true;";
        }
        else
        {
            connectionString = $"Server={serverName};Database={databaseName};Integrated Security=true;TrustServerCertificate=true";
        }

        // Створення контексту бази даних
        using (var checkdbContext = new DbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options))
        {
            // Перевірка існування бази даних
            bool databaseExists = checkdbContext.Database.CanConnect();
            if (databaseExists)
            {
                Console.WriteLine($"База даних {databaseName} вже існує. Бажаєте змінити ім'я бази даних? Якщо не змінимо - то відбудеться перезапис. Якщо змінимо - створиться нова база. (y/n): \n");
                bool renameDatabase = Console.ReadLine()?.ToLower() == "y";

                if (renameDatabase)
                {
                    Console.Write("Введіть нове ім'я бази даних: \n");
                    string? newDatabaseName = Console.ReadLine();
                    if (newDatabaseName != null)
                    {
                        connectionString = RenameDatabase(connectionString, newDatabaseName);
                        Console.WriteLine($"Рядок підключення змінено з в рахуванням того, що ім'я бази даних тепер: {newDatabaseName} \n");
                        Console.WriteLine($"Новий рядок підключення: {connectionString} \n");
                        // змінюємо рядок підключення в конфігураційному файлі (нове ім'я бази)
                        string directory_path = Directory.GetCurrentDirectory();
                        ChangeConfigFile(directory_path, "ConnectionStrings:DefaultConnection", connectionString);
                        string? parentDirectory = Directory.GetParent(directory_path)?.FullName;
                        ChangeConfigFile(directory_path, "ConnectionStrings:DefaultConnection", connectionString);
                    }
                    else
                    {
                        Console.WriteLine($"Не введено нове ім'я бази даних \n");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Такої бази даних {databaseName} не має. Продовжуємо!");
                // змінюємо рядок підключення в конфігураційному файлі (нове ім'я бази даних)
                string directory_path = Directory.GetCurrentDirectory();
                ChangeConfigFile(directory_path, "ConnectionStrings:DefaultConnection", connectionString);
                string? parentDirectory = Directory.GetParent(directory_path)?.FullName;
                ChangeConfigFile(parentDirectory, "ConnectionStrings:DefaultConnection", connectionString);
            }
        }

        using (var context = new DriversManagementContext())
        {
            try
            {
                context.Database.EnsureCreated(); // створюємо БД
                Console.WriteLine($"База даних {databaseName} успішно створена! \n");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Все пішло шкереберть!!! \n");
                Console.WriteLine(ex.ToString());
            }
        }

        Console.WriteLine("Task is finished...\n");
        Task.Delay(2000).Wait();

        Console.WriteLine("Press Enter to continue...");
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///ДОПОМІЖНІ МЕТОДИ 
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Приховування вводимого паролю. По Enter завершується ввід
    /// </summary>
    /// <returns></returns>
    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, (password.Length - 1));
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();

        return password;
    }

    /// <summary>
    /// Зміна імені бази даних у рядку піключення
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="newDatabaseName"></param>
    /// <returns></returns>
    static string RenameDatabase(string? connectionString, string? newDatabaseName)
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
        builder.InitialCatalog = newDatabaseName;
        string newConnectionString = builder.ToString();
        Console.WriteLine($"Ім'я бази даних успішно змінено на {newDatabaseName}.");
        return newConnectionString;
    }

    /// <summary>
    /// Пошук теки в якій знаходиться файл із певним розширенням
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    static string? FindFileWithExtension(string directory, string extension)
    {
        string currentDirectory = directory;
        string[] files = Directory.GetFiles(currentDirectory, $"*.{extension}");

        if (files.Length > 0)
        {
            return files[0];
        }
        else
        {
            string? parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (parentDirectory != null)
            {
                return FindFileWithExtension(parentDirectory, extension);
            }
            else
            {
                return null;
            }
        }
    }
    
    /// <summary>
    /// Зміна конфігураційного файлу
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="directory_path"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    static void ChangeConfigFile<T>(string directory_path, string key, T value)
    {

        try
        {
            string? filePath = FindFileWithExtension(directory_path, "json");
            string json = File.ReadAllText(filePath);
            dynamic? jsonObj = JsonConvert.DeserializeObject(json);

            var sectionPath = key.Split(":")[0];

            if (!string.IsNullOrEmpty(sectionPath))
            {
                var keyPath = key.Split(":")[1];
                jsonObj[sectionPath][keyPath] = value;
            }
            else
            {
                jsonObj[sectionPath] = value;
            }

            string output =JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(filePath, output);
        }
        catch (Exception)
        {
            Console.WriteLine("Error writing app settings");
        }
        Console.WriteLine("Connection string збережено appsettings.json.");
    }
}