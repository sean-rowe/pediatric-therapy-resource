using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

/// <summary>
/// Step definitions for WebSocket-related scenarios
/// These handle real-time communication testing
/// </summary>
[Binding]
    public class WebSocketSteps : BaseStepDefinitions
{
    private ClientWebSocket? _webSocket;
    private List<string> _receivedMessages = new();
    private CancellationTokenSource _cancellationTokenSource = new();

    public WebSocketSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [When(@"I connect to WebSocket ""(.*)"" with auth token")]
    public async Task WhenIConnectToWebSocketWithAuthToken(string endpoint)
    {
        // This will FAIL initially - WebSocket endpoint doesn't exist yet
        _webSocket = new ClientWebSocket();
        
        // Add auth token to headers if available
        if (ScenarioContext.ContainsKey("AuthToken"))
        {
            _webSocket.Options.SetRequestHeader("Authorization", 
                $"Bearer {ScenarioContext["AuthToken"]}");
        }

        var uri = new Uri($"ws://localhost{endpoint}");
        
        try
        {
            await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);
            ScenarioContext["WebSocketConnected"] = true;
            
            // Start listening for messages in background
            _ = Task.Run(async () => await ListenForMessages());
        }
        catch (Exception ex)
        {
            ScenarioContext["WebSocketError"] = ex.Message;
            ScenarioContext["WebSocketConnected"] = false;
        }
    }

    [Then(@"I send WebSocket message:")]
    public async Task WhenISendWebSocketMessage(Table table)
    {
        _webSocket.Should().NotBeNull();
        _webSocket!.State.Should().Be(WebSocketState.Open);

        var message = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            message[row["Field"]] = row["Value"];
        }
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        ScenarioContext["LastWebSocketMessage"] = json;
    }
    [Then(@"I should receive message:")]
    public async Task ThenIShouldReceiveMessage(Table table)
    {
        // Wait for message to arrive (with timeout)
        var timeout = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;

        while (_receivedMessages.Count == 0 && DateTime.UtcNow - startTime < timeout)
        {
            await Task.Delay(100);
        }

        _receivedMessages.Should().NotBeEmpty("Should have received at least one message");

        var lastMessage = _receivedMessages.Last();
        var messageData = JsonSerializer.Deserialize<Dictionary<string, object>>(lastMessage);

        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var expectedValue = row["Value"];
            
            messageData.Should().ContainKey(field);
            messageData![field].ToString().Should().Be(expectedValue);
        }
    }

    [Then(@"WebSocket connection should be active")]
    public void ThenWebSocketConnectionShouldBeActive()
    {
        _webSocket.Should().NotBeNull();
        _webSocket!.State.Should().Be(WebSocketState.Open);
        ScenarioContext["WebSocketActive"] = true;
    }
    [Then(@"I should receive real-time updates")]
    public async Task ThenIShouldReceiveRealTimeUpdates()
    {
        // Wait for at least one update
        var timeout = TimeSpan.FromSeconds(10);
        var startTime = DateTime.UtcNow;

        while (_receivedMessages.Count == 0 && DateTime.UtcNow - startTime < timeout)
        {
            await Task.Delay(100);
        }

        _receivedMessages.Should().NotBeEmpty("Should receive real-time updates");
        ScenarioContext["RealTimeUpdatesReceived"] = true;
    }
    [When(@"I disconnect from WebSocket")]
    public async Task WhenIDisconnectFromWebSocket()
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                "Test completed", _cancellationTokenSource.Token);
        }
        
        ScenarioContext["WebSocketConnected"] = false;
    }
    [Then(@"WebSocket should handle reconnection")]
    public async Task ThenWebSocketShouldHandleReconnection()
    {
        // Simulate disconnection
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                "Testing reconnection", _cancellationTokenSource.Token);
        }

        // Wait a moment
        await Task.Delay(1000);

        // Attempt reconnection
        _webSocket = new ClientWebSocket();
        var uri = new Uri($"ws://localhost{ScenarioContext["LastWebSocketEndpoint"]}");
        
        try
        {
            await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);
            _webSocket.State.Should().Be(WebSocketState.Open);
            ScenarioContext["WebSocketReconnected"] = true;
        }
        catch
        {
            ScenarioContext["WebSocketReconnected"] = false;
        }
    }

    private async Task ListenForMessages()
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        
        while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                    _receivedMessages.Add(message);
                    ScenarioContext["LastReceivedMessage"] = message;
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                string.Empty, _cancellationTokenSource.Token);
                    break;
                }
            }
            catch (Exception ex)
            {
                ScenarioContext["WebSocketListenError"] = ex.Message;
                break;
            }
        }
    }

    [AfterScenario]
    public async Task CleanupWebSocket()
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                "Scenario completed", CancellationToken.None);
        }
        
        _webSocket?.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
