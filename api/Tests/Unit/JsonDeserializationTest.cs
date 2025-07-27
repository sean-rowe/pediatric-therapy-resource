using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UPTRMS.Api.Models.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace UPTRMS.Api.Tests.Unit;

public class JsonDeserializationTest
{
    private readonly ITestOutputHelper _output;

    public JsonDeserializationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void LoginRequest_Should_Deserialize_With_Lowercase_Fields()
    {
        // Test JSON with lowercase field names (as sent by BDD tests)
        string json = """{"email":"user@clinic.com","password":"SecurePass123!"}""";
        
        // Test with case-insensitive options
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        LoginRequest? request = JsonSerializer.Deserialize<LoginRequest>(json, options);
        
        Assert.NotNull(request);
        Assert.Equal("user@clinic.com", request.Email);
        Assert.Equal("SecurePass123!", request.Password);
        
        _output.WriteLine($"Deserialized successfully - Email: {request.Email}, Password: {request.Password}");
    }
    
    [Fact]
    public void LoginRequest_Without_CaseInsensitive_Should_Fail()
    {
        // Test JSON with lowercase field names
        string json = """{"email":"user@clinic.com","password":"SecurePass123!"}""";
        
        // Test WITHOUT case-insensitive options (default)
        JsonSerializerOptions options = new JsonSerializerOptions();
        
        LoginRequest? request = JsonSerializer.Deserialize<LoginRequest>(json, options);
        
        // Without case-insensitive, the properties should be empty/null
        Assert.NotNull(request);
        Assert.Equal(string.Empty, request.Email); // Default value
        Assert.Equal(string.Empty, request.Password); // Default value
        
        _output.WriteLine($"Without case-insensitive - Email: '{request.Email}', Password: '{request.Password}'");
    }
}