using System;

namespace SharedKernel.Messages
{
    /// <summary>
    /// ====================================================================================
    /// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: СПІЛЬНИЙ КОНТРАКТ ДОМЕННОЇ ПОДІЇ (DOMAIN EVENT CONTRACT)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Контракт події "Створено водія" (DriverCreated). Публікується мікросервісом DriversManagement
    /// у шину MassTransit/RabbitMQ та споживається мікросервісами DriversBlogManagement
    /// і CargoOrderingService для створення локальних тіньових реплік (Shadow Drivers).
    ///
    /// ЧОМУ ВИКОРИСТОВУЄТЬСЯ ІНТЕРФЕЙС (IDriverCreated)?
    /// 1. Рекомендація творців MassTransit:
    ///    Шина повідомлень створює RabbitMQ Exchange з назвою за повним кваліфікованим іменем
    ///    інтерфейсу (SharedKernel.Messages:IDriverCreated).
    /// 2. Слабкий зв'язок (Loose Coupling):
    ///    Видавець і споживачі залежать лише від легковагої бібліотеки SharedKernel
    ///    без посилання на важкі бізнес-збірки чи бази даних один одного.
    /// 3. Поліморфна диспетчеризація:
    ///    MassTransit може створювати динамічні проксі при виклику Publish<IDriverCreated>(new { ... }),
    ///    що зменшує шаблонний код.
    /// ====================================================================================
    /// </summary>
    public interface IDriverCreated
    {
        /// <summary>
        /// Первинний унікальний ключ водія (спільний для всіх Bounded Contexts).
        /// </summary>
        public Guid DriverId { get; set; }

        /// <summary>
        /// Ім'я водія.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Прізвище водія.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Номер водійського посвідчення (для ідентифікації в системі вантажоперевезень).
        /// </summary>
        public string DriverLicenseNumber { get; set; }

        /// <summary>
        /// Контактний телефон для логістичних сповіщень.
        /// </summary>
        public string Phone { get; set; }
    }

    /// <summary>
    /// Конкретна реалізація контракту події для екземплярів повідомлення.
    /// </summary>
    public class DriverCreated : IDriverCreated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string Phone { get; set; }
    }
}
