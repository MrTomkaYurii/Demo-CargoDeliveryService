using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Drivers.DAL.ADO.Repositories;

public class PhotoRepository : GenericRepository<Photo>, IPhotoRepository
{
    public PhotoRepository(SqlConnection sqlConnection, IDbTransaction dbtransaction) : base(sqlConnection, dbtransaction, "Photos")
    {

    }
}
