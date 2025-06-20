using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

// Concrete implementation for testing the abstract BaseRepository
public class TestRepository : BaseRepository
{
    public TestRepository(ISecureConfigurationService secureConfiguration, ILogger logger)
        : base(secureConfiguration, logger)
    {
    }

    // Expose protected members for testing
    public string GetConnectionString() => ConnectionString;
    public ILogger GetLogger() => Logger;
    public SqlConnection GetConnection() => CreateConnection();
}

public class BaseRepositoryTests
{
    private readonly Mock<ISecureConfigurationService> _mockSecureConfiguration;
    private readonly Mock<ILogger> _mockLogger;
    private readonly string _testConnectionString = "Server=localhost;Database=TestDb;Integrated Security=true;";

    public BaseRepositoryTests()
    {
        _mockSecureConfiguration = new Mock<ISecureConfigurationService>();
        _mockLogger = new Mock<ILogger>();

        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(_testConnectionString);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldSetConnectionStringAndLogger()
    {
        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.Equal(_testConnectionString, repository.GetConnectionString());
        Assert.Equal(_mockLogger.Object, repository.GetLogger());
    }

    [Fact]
    public void Constructor_WithNullSecureConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TestRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TestRepository(_mockSecureConfiguration.Object, null!));
    }

    [Fact]
    public void CreateConnection_ShouldReturnSqlConnectionWithCorrectConnectionString()
    {
        // Arrange
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Act
        using var connection = repository.GetConnection();

        // Assert
        Assert.IsType<SqlConnection>(connection);
        Assert.Equal(_testConnectionString, connection.ConnectionString);
    }

    [Fact]
    public void CreateConnection_ShouldReturnNewInstanceEachTime()
    {
        // Arrange
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Act
        using var connection1 = repository.GetConnection();
        using var connection2 = repository.GetConnection();

        // Assert
        Assert.NotSame(connection1, connection2);
        Assert.Equal(connection1.ConnectionString, connection2.ConnectionString);
    }

    [Fact]
    public void Constructor_ShouldCallGetConnectionStringOnce()
    {
        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        _mockSecureConfiguration.Verify(
            x => x.GetConnectionString("DefaultConnection"),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenGetConnectionStringThrows_ShouldPropagateException()
    {
        // Arrange
        var exception = new InvalidOperationException("Connection string not found");
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Throws(exception);

        // Act & Assert
        var thrownException = Assert.Throws<InvalidOperationException>(
            () => new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object));
        
        Assert.Equal(exception.Message, thrownException.Message);
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ShouldStillCreateRepository()
    {
        // Arrange
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(string.Empty);

        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.Equal(string.Empty, repository.GetConnectionString());
    }

    [Fact]
    public void Constructor_WithNullConnectionString_ShouldHandleGracefully()
    {
        // Arrange
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns((string?)null);

        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.Null(repository.GetConnectionString());
    }

    [Theory]
    [InlineData("Server=localhost;Database=Test1;")]
    [InlineData("Server=remote;Database=Test2;User Id=user;Password=pass;")]
    [InlineData("Data Source=.;Initial Catalog=TestDb;Integrated Security=True;")]
    public void Constructor_WithVariousConnectionStrings_ShouldStoreCorrectly(string connectionString)
    {
        // Arrange
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(connectionString);

        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.Equal(connectionString, repository.GetConnectionString());
    }

    [Fact]
    public void CreateConnection_WithNullConnectionString_ShouldCreateConnectionWithNullString()
    {
        // Arrange
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns((string?)null);

        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Act
        using var connection = repository.GetConnection();

        // Assert
        Assert.IsType<SqlConnection>(connection);
        Assert.Equal(string.Empty, connection.ConnectionString); // SqlConnection converts null to empty string
    }

    [Fact]
    public void CreateConnection_WithInvalidConnectionString_ShouldCreateConnectionWithInvalidString()
    {
        // Arrange
        var invalidConnectionString = "InvalidConnectionString";
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(invalidConnectionString);

        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Act
        using var connection = repository.GetConnection();

        // Assert
        Assert.IsType<SqlConnection>(connection);
        Assert.Equal(invalidConnectionString, connection.ConnectionString);
        // Note: The connection won't fail until you try to open it
    }

    [Fact]
    public void BaseRepository_ShouldBeAbstractClass()
    {
        // Act & Assert
        Assert.True(typeof(BaseRepository).IsAbstract);
    }

    [Fact]
    public void BaseRepository_ShouldHaveProtectedMembers()
    {
        // Act & Assert
        var connectionStringField = typeof(BaseRepository).GetField("ConnectionString", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var loggerField = typeof(BaseRepository).GetField("Logger", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var createConnectionMethod = typeof(BaseRepository).GetMethod("CreateConnection",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(connectionStringField);
        Assert.NotNull(loggerField);
        Assert.NotNull(createConnectionMethod);
        
        Assert.True(connectionStringField.IsFamily); // protected
        Assert.True(loggerField.IsFamily); // protected
        Assert.True(createConnectionMethod.IsFamily); // protected
    }

    [Fact]
    public void Constructor_ShouldAcceptILoggerInterface()
    {
        // This test verifies that the constructor accepts ILogger (interface) not a concrete type
        // The Mock<ILogger> in our tests proves this works
        
        // Act
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ConnectionString_ShouldBeReadonlyAfterConstruction()
    {
        // Arrange
        var repository = new TestRepository(_mockSecureConfiguration.Object, _mockLogger.Object);
        var initialConnectionString = repository.GetConnectionString();

        // Act - Try to change the secure configuration return value (shouldn't affect existing instance)
        _mockSecureConfiguration
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("NewConnectionString");

        // Assert - Repository should still have the original connection string
        Assert.Equal(initialConnectionString, repository.GetConnectionString());
    }
}