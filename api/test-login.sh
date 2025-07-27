#!/bin/bash

# Build and run the API in the background
dotnet run --project . &
API_PID=$!

# Wait for API to start
echo "Waiting for API to start..."
sleep 5

# Test login with curl
echo "Testing login endpoint..."
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPass123!"}' \
  -v

# Kill the API
kill $API_PID

echo "Test complete"