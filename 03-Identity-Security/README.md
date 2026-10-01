# 🛡️ 03-Identity-Security: Authentication, Authorization & Identity Providers

> Підсистема безпеки, автентифікації та авторизації платформи, що забезпечує захист API та управління доступом користувачів за стандартами **OAuth 2.0** та **OpenID Connect (OIDC)**.

---

## 📂 Структура проєктів блоку

| Проєкт | Тип / Роль | Опис |
| :--- | :--- | :--- |
| **`AuthorizationModuleAPI`** | REST API Security Gateway | ASP.NET Core Web API сервіс для видачі та валідації JWT Bearer токенів, керування ролями, користувачами та політиками доступу |
| **`DuendeIdentityInMemory`** | IdentityServer (OIDC Provider) | Реалізація сервера ідентифікації на базі **Duende IdentityServer** з in-memory клієнтами, ресурсами та тестовими користувачами |
| **`KeycloakPulumiAuth`** | Infrastructure as Code (IaC) | Автоматизація розгортання та конфігурації реалмів (Realms), клієнтів (Clients) та скоупів (Scopes) у **Keycloak** за допомогою **Pulumi C# SDK** |

---

## 🛠️ Інфраструктура авторизації (Docker)

Для повноцінної роботи Keycloak у корені рішення налаштовано `docker-compose.yaml`:
- **Keycloak DB (PostgreSQL):** Порт `5433`
- **Keycloak Server:** Порт `5000` (Legacy endpoint `8080`)
- **Jaeger Tracing:** Порт `16686` (UI)

Запуск інфраструктури:
```powershell
docker compose up -d keycloakdb keycloak
```
