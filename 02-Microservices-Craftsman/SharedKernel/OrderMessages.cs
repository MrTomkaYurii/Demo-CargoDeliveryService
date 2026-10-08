using System;

namespace SharedKernel.Messages
{
    /// <summary>
    /// ====================================================================================
    /// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: ДОМЕННА ПОДІЯ СТВОРЕННЯ ЗАМОВЛЕННЯ (ORDER CREATED CONTRACT)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Контракт події "Нове замовлення створено в логістичній системі" (OrderCreated).
    /// Публікується мікросервісом CargoOrderingService у шину MassTransit/RabbitMQ
    /// для сповіщення зовнішніх підсистем (білінг, трекінг, нотифікації клієнта).
    ///
    /// ЧОМУ ІНТЕРФЕЙС (IOrderCreated)?
    /// 1. Decoupling (Слабкий зв'язок): Видавці та споживачі взаємодіють через спільний контракт,
    ///    не маючи прямих залежностей від внутрішніх структур БД один одного.
    /// 2. MassTransit Topology: За замовчуванням MassTransit створює Fanout Exchange
    ///    за назвою контракту інтерфейсу (або з явним перейменуванням в EndpointRegistration).
    /// ====================================================================================
    /// </summary>
    public interface IOrderCreated
    {
        /// <summary>Унікальний системний ідентифікатор замовлення (GUID).</summary>
        public Guid OrderId { get; set; }

        /// <summary>Бізнес-номер накладної або замовлення (наприклад, "ORD-2026-0042").</summary>
        public string OrderNumber { get; set; }

        /// <summary>ПІБ або назва замовника доставки.</summary>
        public string CustomerName { get; set; }

        /// <summary>Загальна вартість доставки (рядкове або числове представлення).</summary>
        public string TotalAmount { get; set; }
    }

    /// <summary>
    /// Конкретна реалізація контракту події створення замовлення для відправки в шину.
    /// </summary>
    public class OrderCreatedMessage : IOrderCreated
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmount { get; set; }
    }

    /// <summary>
    /// ====================================================================================
    /// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: ДОМЕННА ПОДІЯ ЗАКРІПЛЕННЯ ВОДІЯ ЗА РЕЙСОМ
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Подія зв'язування замовлення з конкретним водієм у логістичному ланцюжку.
    /// Сповіщає мобільні застосунки водіїв та системи GPS-моніторингу про початок виконання.
    /// ====================================================================================
    /// </summary>
    public interface IOrderAssignedToDriver
    {
        /// <summary>Ідентифікатор призначеного замовлення.</summary>
        public Guid OrderId { get; set; }

        /// <summary>Ідентифікатор призначеного водія.</summary>
        public Guid DriverId { get; set; }

        /// <summary>Часова мітка призначення в UTC.</summary>
        public DateTime AssignedAtUtc { get; set; }
    }

    /// <summary>
    /// Конкретна реалізація події закріплення водія за замовленням.
    /// </summary>
    public class OrderAssignedToDriverMessage : IOrderAssignedToDriver
    {
        public Guid OrderId { get; set; }
        public Guid DriverId { get; set; }
        public DateTime AssignedAtUtc { get; set; }
    }
}

