using System.Reflection;
using System.Text;
using System.Data;
using Dapper;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Drivers.DAL.ADO.Repositories.Contracts;

namespace Drivers.DAL.ADO.Repositories;

/// <summary>
/// Базовий узагальнений репозиторій для роботи з базою даних через ADO.NET та Dapper.
/// Забезпечує типові CRUD-операції з автоматичною генерацією параметризованих SQL-запитів.
/// </summary>
/// <typeparam name="T">Тип сутності</typeparam>
public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly SqlConnection _sqlConnection;
    protected readonly IDbTransaction _dbTransaction;
    private readonly string _tableName;

    protected GenericRepository(
        SqlConnection sqlConnection,
        IDbTransaction dbTransaction,
        string tableName)
    {
        _sqlConnection = sqlConnection;
        _dbTransaction = dbTransaction;
        _tableName = tableName;
    }

    /// <summary>
    /// Отримання всіх записів таблиці
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _sqlConnection.QueryAsync<T>(
            $"SELECT * FROM [{_tableName}] WHERE [DeletedAt] IS NULL OR [DeletedAt] IS NOT NULL",
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Отримання сутності за її первинним ключем
    /// </summary>
    public async Task<T> GetAsync(int id)
    {
        var result = await _sqlConnection.QuerySingleOrDefaultAsync<T>(
            $"SELECT * FROM [{_tableName}] WHERE [Id] = @Id",
            param: new { Id = id },
            transaction: _dbTransaction);

        if (result == null)
            throw new KeyNotFoundException($"{_tableName} з Id [{id}] не знайдено.");

        return result;
    }

    /// <summary>
    /// Видалення запису за ідентифікатором
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        await _sqlConnection.ExecuteAsync(
            $"DELETE FROM [{_tableName}] WHERE [Id] = @Id",
            param: new { Id = id },
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Додавання нової сутності з поверненням згенерованого SCOPE_IDENTITY()
    /// </summary>
    public async Task<int> AddAsync(T t)
    {
        var insertQuery = GenerateInsertQuery();
        var newId = await _sqlConnection.ExecuteScalarAsync<int>(
            insertQuery,
            param: t,
            transaction: _dbTransaction);
        return newId;
    }

    /// <summary>
    /// Множинне додавання сутностей
    /// </summary>
    public async Task<int> AddRangeAsync(IEnumerable<T> list)
    {
        var query = GenerateInsertQuery();
        return await _sqlConnection.ExecuteAsync(
            query,
            param: list,
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Оновлення існуючої сутності
    /// </summary>
    public async Task ReplaceAsync(T t)
    {
        var updateQuery = GenerateUpdateQuery();
        await _sqlConnection.ExecuteAsync(
            updateQuery,
            param: t,
            transaction: _dbTransaction);
    }

    private IEnumerable<PropertyInfo> GetProperties => typeof(T).GetProperties();

    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset)
            || underlying == typeof(TimeSpan)
            || underlying == typeof(Guid)
            || underlying == typeof(byte[]);
    }

    private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
    {
        return (from prop in listOfProperties
                where IsSimpleType(prop.PropertyType)
                let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
                select prop.Name).ToList();
    }

    private string GenerateUpdateQuery()
    {
        var updateQuery = new StringBuilder($"UPDATE [{_tableName}] SET ");
        var properties = GenerateListOfProperties(GetProperties);
        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                updateQuery.Append($"[{property}] = @{property}, ");
            }
        });
        updateQuery.Remove(updateQuery.Length - 2, 2);
        updateQuery.Append(" WHERE [Id] = @Id");
        return updateQuery.ToString();
    }

    private string GenerateInsertQuery()
    {
        var insertQuery = new StringBuilder($"INSERT INTO [{_tableName}] (");
        var properties = GenerateListOfProperties(GetProperties);
        properties.RemoveAll(p => p.Equals("Id", StringComparison.OrdinalIgnoreCase));

        properties.ForEach(prop => { insertQuery.Append($"[{prop}], "); });
        insertQuery
            .Remove(insertQuery.Length - 2, 2)
            .Append(") VALUES (");

        properties.ForEach(prop => { insertQuery.Append($"@{prop}, "); });
        insertQuery
            .Remove(insertQuery.Length - 2, 2)
            .Append("); SELECT CAST(SCOPE_IDENTITY() as int);");

        return insertQuery.ToString();
    }
}
