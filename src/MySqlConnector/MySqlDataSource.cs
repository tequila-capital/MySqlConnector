using MySqlConnector.Core;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector;

public sealed class MySqlDataSource : DbDataSource
{
	public MySqlDataSource(string connectionString)
	{
		m_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		Pool = ConnectionPool.CreatePool(m_connectionString);
		m_id = Interlocked.Increment(ref s_lastId);
		m_logArguments = new object?[2] { m_id, null };
		if (Pool is not null)
		{
			m_logArguments[1] = Pool.Id;
			Log.Info("DataSource{0} created with Pool {1}", m_logArguments);
		}
		else
		{
			Log.Info("DataSource{0} created with no pool", m_logArguments);
		}
	}

	public new MySqlConnection GetConnection() => (MySqlConnection) GetDbConnection();

	public new MySqlConnection OpenConnection()
	{
		var connection = GetConnection();
		connection.Open();
		return connection;
	}

	public new async ValueTask<MySqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
	{
		var connection = GetConnection();
		await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
		return connection;
	}

	public override string ConnectionString => m_connectionString;

	public override void Dispose() => DisposeAsync(IOBehavior.Synchronous).GetAwaiter().GetResult();
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
	public override ValueTask DisposeAsync() =>
#else
	public Task DisposeAsync() =>
#endif
		DisposeAsync(IOBehavior.Asynchronous);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
	private async ValueTask DisposeAsync(IOBehavior ioBehavior)
#else
	private async Task DisposeAsync(IOBehavior ioBehavior)
#endif
	{
		if (Pool is not null)
			await Pool.ClearAsync(ioBehavior, default).ConfigureAwait(false);
		m_isDisposed = true;
	}

	protected override DbConnection GetDbConnection()
	{
		if (m_isDisposed)
			throw new ObjectDisposedException(nameof(MySqlDataSource));
		return new MySqlConnection(this);
	}

	internal ConnectionPool? Pool { get; }

	private static readonly IMySqlConnectorLogger Log = MySqlConnectorLogManager.CreateLogger(nameof(MySqlDataSource));
	private static int s_lastId;

	private readonly int m_id;
	private readonly object?[] m_logArguments;
	private readonly string m_connectionString;
	private bool m_isDisposed;
}
