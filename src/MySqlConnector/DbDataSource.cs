namespace System.Data;

public abstract class DbDataSource
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
	 : IDisposable, IAsyncDisposable
#else
	: IDisposable
#endif
{
	public abstract string ConnectionString { get; }

	protected abstract DbConnection GetDbConnection();

	protected virtual DbConnection OpenDbConnection()
	{
		var connection = GetDbConnection();
		connection.Open();

		return connection;
	}

	protected virtual async ValueTask<DbConnection> OpenDbConnectionAsync(CancellationToken cancellationToken = default)
	{
		var connection = GetDbConnection();
		await connection.OpenAsync(cancellationToken);

		return connection;
	}

	protected virtual DbCommand CreateDbCommand() => throw new NotImplementedException();

	//// protected virtual DbCommand CreateDbBatch() ...

	public DbConnection GetConnection() => GetDbConnection();
	public DbConnection OpenConnection() => OpenDbConnection();
	public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default) => OpenDbConnectionAsync(cancellationToken);

	public abstract void Dispose();

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
	public abstract ValueTask DisposeAsync();
#endif
}
