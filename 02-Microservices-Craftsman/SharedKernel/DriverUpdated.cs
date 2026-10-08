using System;

namespace SharedKernel.Messages
{
    /// <summary>
    /// ====================================================================================
    /// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: ДОМЕННА ПОДІЯ ОНОВЛЕННЯ ДАНИХ (EVENTUAL CONSISTENCY)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Контракт події "Водія оновлено" (DriverUpdated).
    /// Публікується мікросервісом DriversManagement після успішного збереження змін
    /// у локальній БД для реплікації нового ПІБ в усі залежні мікросервіси.
    ///
    /// ПРИНЦИП EVENTUAL CONSISTENCY (КІНЦЕВА УЗГОДЖЕНІСТЬ):
    /// У розподіленій системі немає двофазних блокуючих розподілених транзакцій (2PC).
    /// Замість цього DriversManagement фіксує зміну миттєво, а підписники (Blog, Ordering)
    /// наздоганяють цей стан асинхронно за мілісекунди через черги RabbitMQ.
    /// ====================================================================================
    /// </summary>
    public interface IDriverUpdated
    {
        /// <summary>
        /// Ідентифікатор оновленого водія.
        /// </summary>
        public Guid DriverId { get; set; }

        /// <summary>
        /// Оновлене ім'я водія.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Оновлене прізвище водія.
        /// </summary>
        public string LastName { get; set; }
    }

    /// <summary>
    /// Реалізація контракту події оновлення водія.
    /// </summary>
    public class DriverUpdated : IDriverUpdated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}